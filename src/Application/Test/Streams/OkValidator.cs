using FluentValidation;

namespace Application.Test.Streams
{
    public class StreamValidator : AbstractValidator<StreamQuery>
    {
        public StreamValidator()
        {
            RuleFor(ok => ok.Count)
                .NotEmpty();
        }
    }
}
