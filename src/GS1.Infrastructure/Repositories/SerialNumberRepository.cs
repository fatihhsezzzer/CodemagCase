using GS1.Core.Entities;
using GS1.Core.Interfaces;
using GS1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GS1.Infrastructure.Repositories;


/// SerialNumber repository implementasyonu

public class SerialNumberRepository : Repository<SerialNumber>, ISerialNumberRepository
{
    public SerialNumberRepository(GS1DbContext context) : base(context)
    {
    }

    public async Task<SerialNumber?> GetBySerialAsync(string serial, Guid workOrderId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Serial == serial && s.WorkOrderId == workOrderId);
    }

    public async Task<IEnumerable<SerialNumber>> GetByWorkOrderIdAsync(Guid workOrderId)
    {
        return await _dbSet
            .Where(s => s.WorkOrderId == workOrderId)
            .OrderBy(s => s.Serial)
            .ToListAsync();
    }

    public async Task<IEnumerable<SerialNumber>> GetBySSCCIdAsync(Guid ssccId)
    {
        return await _dbSet
            .Where(s => s.SSCCId == ssccId)
            .OrderBy(s => s.Serial)
            .ToListAsync();
    }

    public async Task<IEnumerable<SerialNumber>> GetUnassignedByWorkOrderAsync(Guid workOrderId)
    {
        return await _dbSet
            .Where(s => s.WorkOrderId == workOrderId && s.SSCCId == null)
            .Where(s => s.Status == SerialNumberStatus.Verified || s.Status == SerialNumberStatus.Generated)
            .OrderBy(s => s.Serial)
            .ToListAsync();
    }

    public async Task<bool> SerialExistsAsync(string serial, Guid workOrderId)
    {
        return await _dbSet.AnyAsync(s => s.Serial == serial && s.WorkOrderId == workOrderId);
    }

    public async Task<long> GetLastSerialNumberAsync(Guid workOrderId)
    {
        var lastSerial = await _dbSet
            .Where(s => s.WorkOrderId == workOrderId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastSerial == null)
            return 0;

        // Serial string'den sayısal değeri çıkar
        if (long.TryParse(lastSerial.Serial, out var serialNumber))
            return serialNumber;

        return 0;
    }
}
