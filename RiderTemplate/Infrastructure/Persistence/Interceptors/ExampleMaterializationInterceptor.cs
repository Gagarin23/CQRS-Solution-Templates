using Domain.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Interceptors;

public class ExampleMaterializationInterceptor : IMaterializationInterceptor
{
    public InterceptionResult<object> CreatingInstance(MaterializationInterceptionData materializationData, InterceptionResult<object> result)
    {
        return result;
    }

    public object CreatedInstance(MaterializationInterceptionData materializationData, object entity)
    {
        return entity;
    }

    public InterceptionResult InitializingInstance(MaterializationInterceptionData materializationData, object entity, InterceptionResult result)
    {
        return result;
    }

    public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        // var ctx = materializationData.Context as DatabaseContext;
        // var provider = ctx.AsServiceProvider();
        // var service = provider.GetService<SomeService>();
        // var entity = instance as Entity;
        // entity.ServiceProperty = service;
        return instance;
    }
}
