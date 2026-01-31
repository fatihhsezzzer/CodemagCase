using GS1.Core.Entities;
using GS1.Core.Interfaces;
using GS1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GS1.Infrastructure.Repositories;


/// WorkOrder repository implementasyonu

public class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(GS1DbContext context) : base(context)
    {
    }

    public async Task<WorkOrder?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(w => w.WorkOrderNumber == orderNumber);
    }

    public async Task<IEnumerable<WorkOrder>> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet
            .Where(w => w.ProductId == productId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkOrder>> GetByStatusAsync(WorkOrderStatus status)
    {
        return await _dbSet
            .Where(w => w.Status == status)
            .Include(w => w.Product)
            .ToListAsync();
    }

    public async Task<WorkOrder?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(w => w.Product)
                .ThenInclude(p => p.Customer)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WorkOrder?> GetWithSerialNumbersAsync(Guid id)
    {
        return await _dbSet
            .Include(w => w.Product)
            .Include(w => w.SerialNumbers)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WorkOrder?> GetWithSSCCsAsync(Guid id)
    {
        return await _dbSet
            .Include(w => w.Product)
            .Include(w => w.SSCCs)
                .ThenInclude(s => s.SerialNumbers)
            .Include(w => w.SSCCs)
                .ThenInclude(s => s.ChildSSCCs)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    
    /// İş emri detayı - Tüm bilgilerle birlikte
    
    public async Task<WorkOrder?> GetFullDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(w => w.Product)
                .ThenInclude(p => p.Customer)
            .Include(w => w.SerialNumbers)
            .Include(w => w.SSCCs)
                .ThenInclude(s => s.SerialNumbers)
            .Include(w => w.SSCCs)
                .ThenInclude(s => s.ChildSSCCs)
                    .ThenInclude(c => c.SerialNumbers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<bool> OrderNumberExistsAsync(string orderNumber, Guid? excludeId = null)
    {
        var query = _dbSet.IgnoreQueryFilters().Where(w => w.WorkOrderNumber == orderNumber);
        
        if (excludeId.HasValue)
            query = query.Where(w => w.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }

    public override async Task<IEnumerable<WorkOrder>> GetAllAsync()
    {
        return await _dbSet
            .Include(w => w.Product)
            .ToListAsync();
    }
}
