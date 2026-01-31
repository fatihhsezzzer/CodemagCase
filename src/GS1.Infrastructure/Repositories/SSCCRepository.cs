using GS1.Core.Entities;
using GS1.Core.Interfaces;
using GS1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GS1.Infrastructure.Repositories;


/// SSCC repository implementasyonu

public class SSCCRepository : Repository<SSCC>, ISSCCRepository
{
    public SSCCRepository(GS1DbContext context) : base(context)
    {
    }

    public async Task<SSCC?> GetBySSCCCodeAsync(string ssccCode)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.SSCCCode == ssccCode);
    }

    public async Task<IEnumerable<SSCC>> GetByWorkOrderIdAsync(Guid workOrderId)
    {
        return await _dbSet
            .Where(s => s.WorkOrderId == workOrderId)
            .Include(s => s.SerialNumbers)
            .ToListAsync();
    }

    public async Task<IEnumerable<SSCC>> GetByTypeAsync(SSCCType type, Guid workOrderId)
    {
        var query = _dbSet.Where(s => s.Type == type && s.WorkOrderId == workOrderId);
        
        // WorkOrder bilgisini her zaman yükle (ItemsPerBox için gerekli)
        query = query.Include(s => s.WorkOrder);
        
        // Palet için kolileri, koli için seri numaralarını yükle
        if (type == SSCCType.Pallet)
        {
            query = query.Include(s => s.ChildSSCCs);
        }
        else if (type == SSCCType.Box)
        {
            query = query.Include(s => s.SerialNumbers);
        }
        
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<SSCC>> GetBoxesByPalletIdAsync(Guid palletId)
    {
        return await _dbSet
            .Where(s => s.ParentSSCCId == palletId && s.Type == SSCCType.Box)
            .Include(s => s.SerialNumbers)
            .ToListAsync();
    }

    public async Task<SSCC?> GetWithChildrenAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.ChildSSCCs)
                .ThenInclude(c => c.SerialNumbers)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SSCC?> GetWithSerialNumbersAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.SerialNumbers)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> SSCCCodeExistsAsync(string ssccCode)
    {
        return await _dbSet.AnyAsync(s => s.SSCCCode == ssccCode);
    }
}
