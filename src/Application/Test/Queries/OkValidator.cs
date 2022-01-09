using FluentValidation;

namespace Application.Test.Queries
{
    public class OkValidator : AbstractValidator<OkQuery>
    {
        public OkValidator()
        {
            RuleFor(ok => ok.TestMsg)
                .NotEmpty();
        }
    }
}
