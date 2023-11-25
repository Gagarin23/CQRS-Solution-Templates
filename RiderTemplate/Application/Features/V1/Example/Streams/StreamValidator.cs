using FluentValidation;

namespace Application.Features.V1.Example.Streams
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
