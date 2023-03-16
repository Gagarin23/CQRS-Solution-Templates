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
    public class InputValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly InputValidator<TRequest>? _validator;

        public InputValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validator = validators.GetFirstTypeMatchOrDefault<InputValidator<TRequest>>();
        }

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
                throw new InputValidationException(validationResults.Errors);
            }

            return await next();
        }
    }
}
