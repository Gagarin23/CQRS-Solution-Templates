using MediatR;

namespace Application.Features.V1.Example.Queries
{
    public class OkQuery : IRequest<string>
    {
        public string TestMsg { get; set; }
    }
}
