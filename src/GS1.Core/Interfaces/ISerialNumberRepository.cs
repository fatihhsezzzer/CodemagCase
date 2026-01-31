using GS1.Core.Entities;

namespace GS1.Core.Interfaces;


/// Serial Number repository interface

public interface ISerialNumberRepository : IRepository<SerialNumber>
{
    Task<SerialNumber?> GetBySerialAsync(string serial, Guid workOrderId);
    Task<IEnumerable<SerialNumber>> GetByWorkOrderIdAsync(Guid workOrderId);
    Task<IEnumerable<SerialNumber>> GetBySSCCIdAsync(Guid ssccId);
    Task<IEnumerable<SerialNumber>> GetUnassignedByWorkOrderAsync(Guid workOrderId);
    Task<bool> SerialExistsAsync(string serial, Guid workOrderId);
    Task<long> GetLastSerialNumberAsync(Guid workOrderId);
}
