using GS1.Core.Entities;
using GS1.Core.Exceptions;
using GS1.Core.Interfaces;
using GS1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GS1.Infrastructure.Services;


/// Agregasyon servisi implementasyonu
/// Ürün → Koli → Palet hiyerarşisi yönetimi

public class AggregationService : IAggregationService
{
    private readonly GS1DbContext _context;
    private readonly IGS1GeneratorService _gs1Generator;
    private static long _ssccCounter = DateTime.Now.Ticks % 1000000000; // Benzersizlik için

    public AggregationService(GS1DbContext context, IGS1GeneratorService gs1Generator)
    {
        _context = context;
        _gs1Generator = gs1Generator;
    }

    
    /// Yeni koli SSCC'si oluşturur
    
    public async Task<SSCC> CreateBoxAsync(Guid workOrderId)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Product)
                .ThenInclude(p => p.Customer)
            .FirstOrDefaultAsync(w => w.Id == workOrderId)
            ?? throw new NotFoundException("WorkOrder", workOrderId);

        var ssccCode = GenerateUniqueSSCC(workOrder.Product.Customer.GLN);
        
        var sscc = new SSCC
        {
            SSCCCode = ssccCode,
            Type = SSCCType.Box,
            WorkOrderId = workOrderId,
            GS1DataMatrix = _gs1Generator.GenerateSSCCDataMatrix(ssccCode)
        };

        await _context.SSCCs.AddAsync(sscc);
        await _context.SaveChangesAsync();

        return sscc;
    }

    
    /// Yeni palet SSCC'si oluşturur
    
    public async Task<SSCC> CreatePalletAsync(Guid workOrderId)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Product)
                .ThenInclude(p => p.Customer)
            .FirstOrDefaultAsync(w => w.Id == workOrderId)
            ?? throw new NotFoundException("WorkOrder", workOrderId);

        var ssccCode = GenerateUniqueSSCC(workOrder.Product.Customer.GLN);
        
        var sscc = new SSCC
        {
            SSCCCode = ssccCode,
            Type = SSCCType.Pallet,
            WorkOrderId = workOrderId,
            GS1DataMatrix = _gs1Generator.GenerateSSCCDataMatrix(ssccCode)
        };

        await _context.SSCCs.AddAsync(sscc);
        await _context.SaveChangesAsync();

        return sscc;
    }

    
    /// Ürünleri (seri numaralarını) koliye ekler
    
    public async Task AddItemsToBoxAsync(Guid boxId, IEnumerable<Guid> serialNumberIds)
    {
        var box = await _context.SSCCs
            .Include(s => s.WorkOrder)
            .FirstOrDefaultAsync(s => s.Id == boxId)
            ?? throw new NotFoundException("SSCC (Box)", boxId);

        if (box.Type != SSCCType.Box)
            throw new ValidationException("Sadece koli tipindeki SSCC'ye ürün eklenebilir");

        var serialNumbers = await _context.SerialNumbers
            .Where(s => serialNumberIds.Contains(s.Id))
            .ToListAsync();

        if (serialNumbers.Count != serialNumberIds.Count())
            throw new ValidationException("Bazı seri numaraları bulunamadı");

        // İş emri kontrolü
        var invalidSerials = serialNumbers.Where(s => s.WorkOrderId != box.WorkOrderId).ToList();
        if (invalidSerials.Any())
            throw new ValidationException("Farklı iş emrine ait seri numaraları koliye eklenemez");

        // Zaten başka kolide olanları kontrol et
        var alreadyAssigned = serialNumbers.Where(s => s.SSCCId != null && s.SSCCId != boxId).ToList();
        if (alreadyAssigned.Any())
            throw new ValidationException($"Bazı seri numaraları zaten başka bir koliye atanmış: {string.Join(", ", alreadyAssigned.Select(s => s.Serial))}");

        // Koli kapasitesi kontrolü
        var currentCount = await _context.SerialNumbers.CountAsync(s => s.SSCCId == boxId);
        if (currentCount + serialNumbers.Count > box.WorkOrder.ItemsPerBox)
            throw new ValidationException($"Koli kapasitesi aşılıyor. Mevcut: {currentCount}, Eklenecek: {serialNumbers.Count}, Limit: {box.WorkOrder.ItemsPerBox}");

        foreach (var serial in serialNumbers)
        {
            serial.SSCCId = boxId;
            serial.Status = SerialNumberStatus.Aggregated;
        }

        await _context.SaveChangesAsync();
    }

    
    /// Kolileri palete ekler
    
    public async Task AddBoxesToPalletAsync(Guid palletId, IEnumerable<Guid> boxIds)
    {
        var pallet = await _context.SSCCs
            .Include(s => s.WorkOrder)
            .FirstOrDefaultAsync(s => s.Id == palletId)
            ?? throw new NotFoundException("SSCC (Pallet)", palletId);

        if (pallet.Type != SSCCType.Pallet)
            throw new ValidationException("Sadece palet tipindeki SSCC'ye koli eklenebilir");

        var boxes = await _context.SSCCs
            .Where(s => boxIds.Contains(s.Id))
            .ToListAsync();

        if (boxes.Count != boxIds.Count())
            throw new ValidationException("Bazı koliler bulunamadı");

        // Tip kontrolü
        var invalidBoxes = boxes.Where(b => b.Type != SSCCType.Box).ToList();
        if (invalidBoxes.Any())
            throw new ValidationException("Sadece koli tipindeki SSCC'ler palete eklenebilir");

        // Boş koli kontrolü
        var emptyBoxIds = boxes.Select(b => b.Id).ToList();
        var boxItemCounts = await _context.SerialNumbers
            .Where(sn => emptyBoxIds.Contains(sn.SSCCId.Value))
            .GroupBy(sn => sn.SSCCId)
            .Select(g => new { SSCCId = g.Key, Count = g.Count() })
            .ToListAsync();

        var emptyBoxes = boxes.Where(b => !boxItemCounts.Any(bc => bc.SSCCId == b.Id)).ToList();
        if (emptyBoxes.Any())
            throw new ValidationException($"Boş koliler palete eklenemez: {string.Join(", ", emptyBoxes.Select(b => b.SSCCCode))}");

        // İş emri kontrolü
        var wrongWorkOrder = boxes.Where(b => b.WorkOrderId != pallet.WorkOrderId).ToList();
        if (wrongWorkOrder.Any())
            throw new ValidationException("Farklı iş emrine ait koliler palete eklenemez");

        // Zaten başka palette olanları kontrol et
        var alreadyAssigned = boxes.Where(b => b.ParentSSCCId != null && b.ParentSSCCId != palletId).ToList();
        if (alreadyAssigned.Any())
            throw new ValidationException($"Bazı koliler zaten başka bir palete atanmış: {string.Join(", ", alreadyAssigned.Select(b => b.SSCCCode))}");

        // Palet kapasitesi kontrolü
        var currentCount = await _context.SSCCs.CountAsync(s => s.ParentSSCCId == palletId);
        if (currentCount + boxes.Count > pallet.WorkOrder.BoxesPerPallet)
            throw new ValidationException($"Palet kapasitesi aşılıyor. Mevcut: {currentCount}, Eklenecek: {boxes.Count}, Limit: {pallet.WorkOrder.BoxesPerPallet}");

        foreach (var box in boxes)
        {
            box.ParentSSCCId = palletId;
        }

        await _context.SaveChangesAsync();
    }

    
    /// Ürünü koliden çıkarır
    
    public async Task RemoveItemFromBoxAsync(Guid serialNumberId)
    {
        var serialNumber = await _context.SerialNumbers
            .FindAsync(serialNumberId)
            ?? throw new NotFoundException("SerialNumber", serialNumberId);

        serialNumber.SSCCId = null;
        serialNumber.Status = SerialNumberStatus.Verified; // Önceki duruma döndür

        await _context.SaveChangesAsync();
    }

    
    /// Koliyi paletten çıkarır
    
    public async Task RemoveBoxFromPalletAsync(Guid boxId)
    {
        var box = await _context.SSCCs
            .FindAsync(boxId)
            ?? throw new NotFoundException("SSCC (Box)", boxId);

        if (box.Type != SSCCType.Box)
            throw new ValidationException("Sadece koli tipindeki SSCC paletten çıkarılabilir");

        box.ParentSSCCId = null;

        await _context.SaveChangesAsync();
    }

    
    /// Agregasyon hiyerarşisini getirir
    
    public async Task<SSCC?> GetAggregationHierarchyAsync(Guid ssccId)
    {
        return await _context.SSCCs
            .Include(s => s.SerialNumbers)
            .Include(s => s.ChildSSCCs)
                .ThenInclude(c => c.SerialNumbers)
            .FirstOrDefaultAsync(s => s.Id == ssccId);
    }

    
    /// Benzersiz SSCC kodu üretir
    
    private string GenerateUniqueSSCC(string gln)
    {
        // GLN'den company prefix al (ilk 7-10 hane)
        var companyPrefix = gln[..9]; // 9 haneli company prefix
        
        // Thread-safe counter artırımı
        var serialRef = Interlocked.Increment(ref _ssccCounter);
        
        return _gs1Generator.GenerateSSCC(companyPrefix, 0, serialRef);
    }
}
