using GS1.Core.Interfaces;
using GS1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace GS1.Infrastructure.Repositories;


/// Unit of Work implementasyonu

public class UnitOfWork : IUnitOfWork
{
    private readonly GS1DbContext _context;
    private IDbContextTransaction? _transaction;

    private ICustomerRepository? _customers;
    private IProductRepository? _products;
    private IWorkOrderRepository? _workOrders;
    private ISerialNumberRepository? _serialNumbers;
    private ISSCCRepository? _ssccs;

    public UnitOfWork(GS1DbContext context)
    {
        _context = context;
    }

    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IWorkOrderRepository WorkOrders => _workOrders ??= new WorkOrderRepository(_context);
    public ISerialNumberRepository SerialNumbers => _serialNumbers ??= new SerialNumberRepository(_context);
    public ISSCCRepository SSCCs => _ssccs ??= new SSCCRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
