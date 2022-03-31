using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface IContextPooledFactory
{
    Task<IDatabaseContext> CreateContextAsync(CancellationToken cancellationToken = default);
    IDatabaseContext CreateContext();
}
