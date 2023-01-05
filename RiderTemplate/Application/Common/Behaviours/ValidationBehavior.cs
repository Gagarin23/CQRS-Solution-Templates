using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Application.Common.Behaviours
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var validationContext = new ValidationContext<TRequest>(request);

                var validationResults =
                    await Task.WhenAll(_validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));

                var failures = validationResults.SelectMany(r => r.Errors)
                    .Where(f => f != null);

                if (failures.Any())
                {
                    var error = new ValidationException(failures);

                    throw error;
                }
            }

            return await next();
        }
    }
}
