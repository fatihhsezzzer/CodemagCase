using GS1.Core.Entities;
using GS1.Core.Interfaces;
using GS1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GS1.Infrastructure.Repositories;


/// Product repository implementasyonu

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(GS1DbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByGTINAsync(string gtin)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.GTIN == gtin);
    }

    public async Task<IEnumerable<Product>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _dbSet
            .Where(p => p.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<Product?> GetWithWorkOrdersAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Customer)
            .Include(p => p.WorkOrders)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> GTINExistsAsync(string gtin, Guid? excludeId = null)
    {
        var query = _dbSet.IgnoreQueryFilters().Where(p => p.GTIN == gtin);
        
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        
        return await query.AnyAsync();
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Customer)
            .ToListAsync();
    }
}
