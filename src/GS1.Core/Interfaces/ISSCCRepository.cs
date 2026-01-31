using GS1.Core.Entities;

namespace GS1.Core.Interfaces;


/// SSCC repository interface

public interface ISSCCRepository : IRepository<SSCC>
{
    Task<SSCC?> GetBySSCCCodeAsync(string ssccCode);
    Task<IEnumerable<SSCC>> GetByWorkOrderIdAsync(Guid workOrderId);
    Task<IEnumerable<SSCC>> GetByTypeAsync(SSCCType type, Guid workOrderId);
    Task<IEnumerable<SSCC>> GetBoxesByPalletIdAsync(Guid palletId);
    Task<SSCC?> GetWithChildrenAsync(Guid id);
    Task<SSCC?> GetWithSerialNumbersAsync(Guid id);
    Task<bool> SSCCCodeExistsAsync(string ssccCode);
}
