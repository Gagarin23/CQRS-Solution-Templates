using System.Collections.Generic;
using System.Runtime.Serialization;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Common.Exceptions;

public class BusinessValidationException : ValidationException
{
    public BusinessValidationException(string sourceProperty, string message)
        : base(message, new []{new ValidationFailure(sourceProperty, message)}) { }

    public BusinessValidationException(string message, IEnumerable<ValidationFailure> errors)
        : base(message, errors) { }

    public BusinessValidationException(string message, IEnumerable<ValidationFailure> errors, bool appendDefaultMessage)
        : base(message, errors,
            appendDefaultMessage
        ) { }

    public BusinessValidationException(IEnumerable<ValidationFailure> errors)
        : base(errors) { }

    public BusinessValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

