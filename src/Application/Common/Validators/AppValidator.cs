using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Validators;

public abstract class AppValidator<T> : AbstractValidator<T>
{
    private Dictionary<object, object> _validationContext;
    protected Dictionary<object, object> ValidationContext => _validationContext ??= new();
    protected IServiceProvider ServiceProvider { get; }
    protected IDistributedCache Cache { get; }

    protected AppValidator(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Cache = ServiceProvider.GetRequiredService<IDistributedCache>();
    }

    protected ValueTask<bool> ValidateWithCachingAsync(string cacheKey, Func<Task<bool>> valueFactory, CancellationToken cancellationToken)
    {
        return Cache.GetOrSetAsync(cacheKey, valueFactory, TimeSpan.FromMinutes(5), cancellationToken);
    }

    protected  ValueTask<bool> ValidateWithCachingAsync(string cacheKey, Func<ValueTask<bool>> valueFactory, CancellationToken cancellationToken)
    {
        return Cache.GetOrSetAsync(cacheKey, valueFactory, TimeSpan.FromMinutes(5), cancellationToken);
    }
}
