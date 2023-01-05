using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Application.Common.Interfaces
{
    public interface IDatabaseContext
    {
        public DatabaseFacade Database { get; }
        public ChangeTracker ChangeTracker { get; }
        public IModel Model { get; }

        IServiceProvider AsServiceProvider();
    }
}
