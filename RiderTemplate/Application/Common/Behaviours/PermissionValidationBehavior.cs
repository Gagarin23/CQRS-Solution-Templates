using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using MediatR;

namespace Application.Common.Behaviours
{
    public class PermissionValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly PermissionValidator<TRequest>? _validator;

        public PermissionValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validator = validators.GetFirstTypeMatchOrDefault<PermissionValidator<TRequest>>();
        }

        /// <summary>
        /// Выполняется проверка разграничения доступа к данным
        /// </summary>
        /// <param name="request"></param>
        /// <param name="next"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="BusinessValidationException"></exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validator == null)
            {
                return await next();
            }

            var validationContext = new ValidationContext<TRequest>(request);

            var validationResults = await _validator.ValidateAsync(validationContext, cancellationToken);

            if (validationResults.Errors.Any())
            {
                throw new ForbiddenValidationException(validationResults.Errors);
            }

            return await next();
        }
    }
}
