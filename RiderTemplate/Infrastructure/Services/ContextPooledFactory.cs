using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ContextPooledFactory : IContextPooledFactory
{
    private readonly IDbContextFactory<DatabaseContext> _pooledFactory;

    public ContextPooledFactory(IDbContextFactory<DatabaseContext> pooledFactory)
    {
        _pooledFactory = pooledFactory;
    }
    
    public async Task<IDatabaseContext> CreateContextAsync(CancellationToken cancellationToken = default)
    {
        return await _pooledFactory.CreateDbContextAsync(cancellationToken);
    }

    public IDatabaseContext CreateContext()
    {
        return _pooledFactory.CreateDbContext();
    }
}
