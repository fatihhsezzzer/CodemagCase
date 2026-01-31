using GS1.Core.Entities;
using GS1.Core.Exceptions;
using GS1.Core.Interfaces;
using GS1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GS1.Infrastructure.Services;


/// Serilizasyon servisi implementasyonu

public class SerializationService : ISerializationService
{
    private readonly GS1DbContext _context;
    private readonly IGS1GeneratorService _gs1Generator;

    public SerializationService(GS1DbContext context, IGS1GeneratorService gs1Generator)
    {
        _context = context;
        _gs1Generator = gs1Generator;
    }

    
    /// İş emri için seri numaraları üretir
    
    public async Task<IEnumerable<SerialNumber>> GenerateSerialNumbersAsync(Guid workOrderId, int quantity)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Product)
            .FirstOrDefaultAsync(w => w.Id == workOrderId)
            ?? throw new NotFoundException("WorkOrder", workOrderId);

        if (workOrder.Status == WorkOrderStatus.Cancelled)
            throw new WorkOrderStatusException("İptal edilmiş iş emri için seri numarası üretilemez");

        if (workOrder.Status == WorkOrderStatus.Completed)
            throw new WorkOrderStatusException("Tamamlanmış iş emri için seri numarası üretilemez");

        // Mevcut seri sayısını kontrol et
        var existingCount = await _context.SerialNumbers
            .CountAsync(s => s.WorkOrderId == workOrderId);
        // Üretim adedini aşan seri numarası kontrolü
        if (existingCount + quantity > workOrder.ProductionQuantity)
            throw new ValidationException(
                $"Üretim adedini aşan seri numarası üretilemez. Mevcut: {existingCount}, İstenen: {quantity}, Limit: {workOrder.ProductionQuantity}");

        var serialNumbers = new List<SerialNumber>();
        var startSequence = workOrder.LastSerialNumber == 0 
            ? workOrder.SerialNumberStart 
            : workOrder.LastSerialNumber + 1;

        for (int i = 0; i < quantity; i++)
        {
            var sequenceNumber = startSequence + i;
            var serial = _gs1Generator.GenerateSerialNumber(sequenceNumber);
            var gs1DataMatrix = _gs1Generator.GenerateGS1DataMatrix(
                workOrder.Product.GTIN,
                serial,
                workOrder.ExpirationDate,
                workOrder.BatchNumber
            );

            var serialNumber = new SerialNumber
            {
                Serial = serial,
                GS1DataMatrix = gs1DataMatrix,
                Status = SerialNumberStatus.Generated,
                WorkOrderId = workOrderId
            };

            serialNumbers.Add(serialNumber);
        }

        await _context.SerialNumbers.AddRangeAsync(serialNumbers);
        
        // İş emrinin son seri numarasını güncelle
        workOrder.LastSerialNumber = startSequence + quantity - 1;
        
        // İş emri durumunu güncelle
        if (workOrder.Status == WorkOrderStatus.Created)
            workOrder.Status = WorkOrderStatus.InProgress;

        // Tüm seri numaraları üretildiyse iş emrini tamamla
        var totalSerialCount = existingCount + quantity;
        if (totalSerialCount >= workOrder.ProductionQuantity)
            workOrder.Status = WorkOrderStatus.Completed;

        await _context.SaveChangesAsync();

        return serialNumbers;
    }

    
    /// Seri numarasını basıldı olarak işaretler
    
    public async Task MarkAsPrintedAsync(Guid serialNumberId)
    {
        var serialNumber = await _context.SerialNumbers
            .FindAsync(serialNumberId)
            ?? throw new NotFoundException("SerialNumber", serialNumberId);

        serialNumber.Status = SerialNumberStatus.Printed;
        serialNumber.PrintedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    
    /// Seri numarasını doğrulandı olarak işaretler
    
    public async Task MarkAsVerifiedAsync(Guid serialNumberId)
    {
        var serialNumber = await _context.SerialNumbers
            .FindAsync(serialNumberId)
            ?? throw new NotFoundException("SerialNumber", serialNumberId);

        serialNumber.Status = SerialNumberStatus.Verified;
        serialNumber.VerifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    
    /// Seri numarasını reddedildi olarak işaretler
    
    public async Task MarkAsRejectedAsync(Guid serialNumberId)
    {
        var serialNumber = await _context.SerialNumbers
            .FindAsync(serialNumberId)
            ?? throw new NotFoundException("SerialNumber", serialNumberId);

        serialNumber.Status = SerialNumberStatus.Rejected;

        await _context.SaveChangesAsync();
    }
}
