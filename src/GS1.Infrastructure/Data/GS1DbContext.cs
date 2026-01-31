using GS1.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GS1.Infrastructure.Data;


/// GS1 Serilizasyon veritabanı context'i

public class GS1DbContext : DbContext
{
    public GS1DbContext(DbContextOptions<GS1DbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<SerialNumber> SerialNumbers => Set<SerialNumber>();
    public DbSet<SSCC> SSCCs => Set<SSCC>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configurations
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new WorkOrderConfiguration());
        modelBuilder.ApplyConfiguration(new SerialNumberConfiguration());
        modelBuilder.ApplyConfiguration(new SSCCConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // UpdatedAt otomatik güncelleme
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            
            throw new Exception($"Veritabanı kaydetme hatası: {ex.Message}", ex);
        }
    }
}
