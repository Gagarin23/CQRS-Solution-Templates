using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Application.Common.Extensions;

public static class ValidationExceptionExtensions
{
    public static IDictionary<string, string[]> GroupErrorsByProperty(this ValidationException exception)
    {
        return exception.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary
            (
                keySelector: failures => failures.Key,
                elementSelector: failures => failures
                    .Select(f => f.ErrorMessage)
                    .ToArray()
            );
    }
}
