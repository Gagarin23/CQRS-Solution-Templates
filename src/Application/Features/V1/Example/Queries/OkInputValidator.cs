using System;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.V1.Example.Queries
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
