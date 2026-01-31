using GS1.Core.Entities;

namespace GS1.Core.Interfaces;


/// Product repository interface

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByGTINAsync(string gtin);
    Task<IEnumerable<Product>> GetByCustomerIdAsync(Guid customerId);
    Task<Product?> GetWithWorkOrdersAsync(Guid id);
    Task<bool> GTINExistsAsync(string gtin, Guid? excludeId = null);
}
