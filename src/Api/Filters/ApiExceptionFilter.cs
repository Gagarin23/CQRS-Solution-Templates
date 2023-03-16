using Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using Application.Common.Extensions;

namespace Api.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

        public ApiExceptionFilter()
        {
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(InputValidationException), HandleInputValidationException },
                { typeof(BusinessValidationException), HandleBusinessValidationException },
                { typeof(AuthenticationException), HandleAuthenticationException }
            };
        }

        public override void OnException(ExceptionContext context)
        {
            var type = context.Exception.GetType();

            if (_exceptionHandlers.TryGetValue(type, out var handle))
            {
                handle(context);
                return;
            }

            HandleUnknownException(context);
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);

            context.ExceptionHandled = true;
        }

        private void HandleInputValidationException(ExceptionContext context)
        {
            var exception = context.Exception as InputValidationException;

            var details = new ValidationProblemDetails(exception.GroupErrorsByProperty());

            details.Title = exception?.Message;

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }

        private void HandleBusinessValidationException(ExceptionContext context)
        {
            var exception = context.Exception as BusinessValidationException;

            var details = new ValidationProblemDetails(exception.GroupErrorsByProperty());

            details.Title = exception?.Message;

            context.Result = new ConflictObjectResult(details);

            context.ExceptionHandled = true;
        }

        private void HandleAuthenticationException(ExceptionContext context)
        {
            var details = new ProblemDetails()
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = context.Exception.Message
            };

            context.Result = new UnauthorizedObjectResult(details);

            context.ExceptionHandled = true;
        }
    }
}
