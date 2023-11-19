using System;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Example.Queries
{
    public class OkInputValidator : InputValidator<OkQuery>
    {
        public OkInputValidator(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            RuleFor(ok => ok.TestMsg)
                .NotEmpty();
        }
    }
}
