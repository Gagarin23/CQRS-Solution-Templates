using System;

namespace Application.Common.Validators;

public abstract class PermissionValidator<T> : AppValidator<T>
{
    protected PermissionValidator(IServiceProvider serviceProvider)
        : base(serviceProvider) { }
}
