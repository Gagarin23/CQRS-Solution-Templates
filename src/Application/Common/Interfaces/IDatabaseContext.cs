using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Common.Interfaces
{
    public interface IDatabaseContext
    {
        DatabaseFacade GetDatabase();
        ChangeTracker GetChangeTracker();
    }
}
