using System.Collections.Generic;
using System.Runtime.Serialization;
using Application.Common.Constants;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Common.Exceptions
{
    public class ForbiddenValidationException : ValidationException
    {
        public ForbiddenValidationException()
            : base(ValidationMessages.UserActionForbidden) { }

        public ForbiddenValidationException(IEnumerable<ValidationFailure> errors)
            : base(ValidationMessages.UserActionForbidden, errors) { }

        public ForbiddenValidationException(IEnumerable<ValidationFailure> errors, bool appendDefaultMessage)
            : base(ValidationMessages.UserActionForbidden, errors,
                appendDefaultMessage
            ) { }

        public ForbiddenValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
