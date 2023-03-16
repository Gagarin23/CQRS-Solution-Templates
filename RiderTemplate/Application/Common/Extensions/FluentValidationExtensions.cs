using System.Net;
using FluentValidation;

namespace Application.Common.Extensions;

public static class FluentValidationExtensions
{
    private static string? _badRequestCode;
    private static string BadRequestCode => _badRequestCode ??= HttpStatusCode.BadRequest.CodeToString();
    private static string? _notFoundStatusCode;
    private static string NotFoundStatusCode => _notFoundStatusCode ??= HttpStatusCode.NotFound.CodeToString();
    private static string? _conflictStatusCode;
    private static string ConflictStatusCode => _conflictStatusCode ??= HttpStatusCode.Conflict.CodeToString();
    
    public static IRuleBuilderOptions<T, TProperty> WithBadRequestCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule)
    {
        return rule.WithErrorCode(BadRequestCode);
    }
    
    public static IRuleBuilderOptions<T, TProperty> WithNotFoundCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule)
    {
        return rule.WithErrorCode(NotFoundStatusCode);
    }
    
    public static IRuleBuilderOptions<T, TProperty> WithConflictCode<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule)
    {
        return rule.WithErrorCode(ConflictStatusCode);
    }
    
    public static string CodeToString(this HttpStatusCode httpStatusCode)
    {
        return ((int)httpStatusCode).ToString();
    }
}
