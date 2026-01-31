namespace GS1.Core.Interfaces;


/// Unit of Work interface - Transaction yönetimi için

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }
    IWorkOrderRepository WorkOrders { get; }
    ISerialNumberRepository SerialNumbers { get; }
    ISSCCRepository SSCCs { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
