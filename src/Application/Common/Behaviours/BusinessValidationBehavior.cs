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
    public class BusinessValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly BusinessValidator<TRequest>? _validator;

        public BusinessValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validator = validators.GetFirstTypeMatchOrDefault<BusinessValidator<TRequest>>();
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
                throw new BusinessValidationException(validationResults.Errors);
            }

            return await next();
        }
    }
}
