using System.Collections.Generic;
using System.Linq;
using Api.Filters;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ValidationExceptionExtensions
{
    public static void AddDetailErrors(this ValidationProblemDetails details, ValidationFailure failure)
    {
            details.Extensions.Add("details", new Dictionary<string, object>(1)
            {
                {
                    failure.PropertyName,
                    new
                    {
                        ErrorCode = int.TryParse(failure.ErrorCode, out var code) ? (int?)code : null,
                        failure.ErrorMessage
                    }
                }
            });
    }

    public static void AddDetailErrors(this ValidationProblemDetails details, IEnumerable<ValidationFailure> failures)
    {
        var errors = failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary
            (
                keySelector: failures => failures.Key,
                elementSelector: failures => failures
                    .Select(failure => new ProblemDetail
                    {
                        ErrorCode = int.TryParse(failure.ErrorCode, out var code) ? code : null,
                        ErrorMessage = failure.ErrorMessage
                    })
                    .ToList()
            );

            details.Extensions.Add("details", errors);
    }
}
