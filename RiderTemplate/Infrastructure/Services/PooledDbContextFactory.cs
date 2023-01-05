using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PooledDbContextFactory : IPooledDbContextFactory
{
    private readonly IDbContextFactory<DatabaseContext> _factory;

    public PooledDbContextFactory(IDbContextFactory<DatabaseContext> factory)
    {
        _factory = factory;
    }
    
    public async Task<IDatabaseContext> CreateContextAsync(CancellationToken cancellationToken = default)
    {
        return await _factory.CreateDbContextAsync(cancellationToken);
    }

    public IDatabaseContext CreateContext()
    {
        return _factory.CreateDbContext();
    }
}
