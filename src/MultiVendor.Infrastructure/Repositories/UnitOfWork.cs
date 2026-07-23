using System.Collections;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using MultiVendor.Domain.Common;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Infrastructure.Persistence;

namespace MultiVendor.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MultiVendorAppDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _transaction;
    private Hashtable _repositories = new();

    public UnitOfWork(MultiVendorAppDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        var key = type.Name;

        if (_repositories.ContainsKey(key))
            return (IRepository<T>)_repositories[key]!;

        var repository = new GenericRepository<T>(_context);
        _repositories.Add(key, repository);
        return repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
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
