using System.Net;
using Application.Common.Exceptions;
using FluentValidation;

namespace Application.Common.Extensions;

public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithBadRequestCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule)
    {
        return rule.WithErrorCode(HttpStatusCode.BadRequest.ValueToString());
    }
    
    public static IRuleBuilderOptions<T, TProperty> WithNotFoundCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule)
    {
        return rule.WithErrorCode(HttpStatusCode.NotFound.ValueToString());
    }
    
    public static IRuleBuilderOptions<T, TProperty> WithConflictCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule)
    {
        return rule.WithErrorCode(HttpStatusCode.Conflict.ValueToString());
    }
}
