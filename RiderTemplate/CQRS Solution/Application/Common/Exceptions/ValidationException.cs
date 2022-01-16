using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }
        
        public ValidationException()
            : base("Validation exception was thrown")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string title, string message)
            : this()
        {
            Errors.Add(title, new string[] { message });
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            var failureGroups = failures
                .GroupBy(failure => failure.PropertyName);

            foreach (var failureGroup in failureGroups)
            {
                var propertyName = failureGroup.Key;
                var propertyFailures = failureGroup
                    .Select(failures => failures.ErrorMessage)
                    .ToArray();

                Errors.Add(propertyName, propertyFailures);
            }
        }
    }
}
