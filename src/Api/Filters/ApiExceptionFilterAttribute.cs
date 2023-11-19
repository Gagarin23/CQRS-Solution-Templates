using Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Api.Extensions;
using Application.Common.Interfaces;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace Api.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private const string DefaultValidationTitle = "Validation failed.";
        private readonly ActivitySource ActivitySource = new(nameof(ApiExceptionFilterAttribute));

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;
        private readonly IDictionary<Type, Func<ExceptionContext, ValueTask>> _exceptionHandlers;

        public ApiExceptionFilterAttribute(IHttpContextAccessor httpContextAccessor, ILogger<ApiExceptionFilterAttribute> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _exceptionHandlers = new Dictionary<Type, Func<ExceptionContext, ValueTask>>
            {
                { typeof(InputValidationException), HandleInputValidationExceptionAsync },
                { typeof(BusinessValidationException), HandleBusinessValidationExceptionAsync },
                { typeof(ForbiddenValidationException), HandleForbiddenValidationExceptionAsync },
                { typeof(AuthenticationException), HandleAuthenticationExceptionAsync }
            };
        }

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            using var activity = ActivitySource.StartActivity();
            if (context.Exception.Message != null)
            {
                _logger.LogError($"{activity.TraceId}\n{context.Exception.Message}\n{context.Exception.StackTrace}\n");
            }

            var type = context.Exception.GetType();

            if (_exceptionHandlers.TryGetValue(type, out var handle))
            {
                await handle(context);
                return;
            }

            await HandleUnknownExceptionAsync(context, activity);
        }

        private async Task HandleUnknownExceptionAsync(ExceptionContext context, Activity activity)
        {
            var message = $"Oops. Incident id: {activity.TraceId}.";
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                httpContext.Response.Clear();
                httpContext.Response.ContentType = "text/plain;charset=utf-8";
                if (EnvironmentExtension.IsDevelopment)
                {
                    await httpContext.Response.WriteAsync(context.Exception.ToString());
                }
                else
                {
                    await httpContext.Response.WriteAsync(message);
                }
                context.ExceptionHandled = true;
            }
            else
            {
                var details = new ProblemDetails()
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = message
                };

                SetTrace(details, context.Exception);

                context.Result = new ObjectResult(details)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };

                context.ExceptionHandled = true;
            }
        }

        private ValueTask HandleInputValidationExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception as InputValidationException;

            var details = new ValidationProblemDetails()
            {
                Title = DefaultValidationTitle
            };

            details.AddDetailErrors(exception.Errors);

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
            return ValueTask.CompletedTask;
        }

        private ValueTask HandleBusinessValidationExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception as BusinessValidationException;

            var details = new ValidationProblemDetails()
            {
                Title = DefaultValidationTitle
            };

            details.AddDetailErrors(exception.Errors);

            context.Result = new ConflictObjectResult(details);

            context.ExceptionHandled = true;
            return ValueTask.CompletedTask;
        }

        private ValueTask HandleForbiddenValidationExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception as ForbiddenValidationException;

            if (exception.Errors.Any(x => string.IsNullOrWhiteSpace(x.ErrorCode)))
            {
                throw new Exception("В ForbiddenValidationException не указан ErrorCode");
            }

            var details = new ValidationProblemDetails()
            {
                Title = exception.Message
            };

            details.AddDetailErrors(exception.Errors);

            context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status403Forbidden };

            context.ExceptionHandled = true;
            return ValueTask.CompletedTask;
        }

        private ValueTask HandleAuthenticationExceptionAsync(ExceptionContext context)
        {
            var details = new ProblemDetails()
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = context.Exception.Message
            };

            context.Result = new UnauthorizedObjectResult(details);

            context.ExceptionHandled = true;
            return ValueTask.CompletedTask;
        }

        private void SetTrace(ProblemDetails details, Exception exception)
        {
            if (EnvironmentExtension.IsDevelopment)
            {
                details.Detail = exception.ToString();
            }
        }
    }
}
