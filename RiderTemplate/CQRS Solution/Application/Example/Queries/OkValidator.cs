using FluentValidation;

namespace Application.Example.Queries
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
