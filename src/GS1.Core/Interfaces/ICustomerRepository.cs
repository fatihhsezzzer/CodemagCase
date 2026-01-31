using GS1.Core.Entities;

namespace GS1.Core.Interfaces;


/// Customer repository interface

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByGLNAsync(string gln);
    Task<Customer?> GetWithProductsAsync(Guid id);
    Task<bool> GLNExistsAsync(string gln, Guid? excludeId = null);
}
