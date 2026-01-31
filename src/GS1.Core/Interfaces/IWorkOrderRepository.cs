using GS1.Core.Entities;

namespace GS1.Core.Interfaces;


/// Work Order repository interface

public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    Task<WorkOrder?> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<WorkOrder>> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<WorkOrder>> GetByStatusAsync(WorkOrderStatus status);
    Task<WorkOrder?> GetWithDetailsAsync(Guid id);
    Task<WorkOrder?> GetWithSerialNumbersAsync(Guid id);
    Task<WorkOrder?> GetWithSSCCsAsync(Guid id);
    Task<WorkOrder?> GetFullDetailsAsync(Guid id);
    Task<bool> OrderNumberExistsAsync(string orderNumber, Guid? excludeId = null);
}
