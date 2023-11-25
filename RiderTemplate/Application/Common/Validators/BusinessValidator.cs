using System;

namespace Application.Common.Validators;

public abstract class BusinessValidator<T> : AppValidator<T>
{
    protected BusinessValidator(IServiceProvider serviceProvider)
        : base(serviceProvider) { }
}
