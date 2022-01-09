using FluentValidation;

namespace Application.Example.Streams
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
