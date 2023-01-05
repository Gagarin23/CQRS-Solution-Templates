using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface IPooledDbContextFactory
{
    Task<IDatabaseContext> CreateContextAsync(CancellationToken cancellationToken = default);
    IDatabaseContext CreateContext();
}
