using GS1.Core.Entities;
using GS1.Core.Interfaces;
using GS1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GS1.Infrastructure.Repositories;


/// Customer repository implementasyonu

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(GS1DbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByGLNAsync(string gln)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.GLN == gln);
    }

    public async Task<Customer?> GetWithProductsAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> GLNExistsAsync(string gln, Guid? excludeId = null)
    {
        var query = _dbSet.IgnoreQueryFilters().Where(c => c.GLN == gln);
        
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }
}
