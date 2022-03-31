using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Common.Interfaces
{
    public interface IDatabaseContext
    {
        public DatabaseFacade Database { get; }
        public ChangeTracker ChangeTracker { get; }
    }
}
