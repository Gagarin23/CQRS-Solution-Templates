using System.Collections.Generic;
using System.Runtime.Serialization;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Common.Exceptions;

public class InputValidationException : ValidationException
{
    public InputValidationException(string sourceProperty, string message)
        : base(message, new []{new ValidationFailure(sourceProperty, message)}) { }

    public InputValidationException(string message, IEnumerable<ValidationFailure> errors)
        : base(message, errors) { }

    public InputValidationException(string message, IEnumerable<ValidationFailure> errors, bool appendDefaultMessage)
        : base(message, errors,
            appendDefaultMessage
        ) { }

    public InputValidationException(IEnumerable<ValidationFailure> errors)
        : base(errors) { }

    public InputValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

