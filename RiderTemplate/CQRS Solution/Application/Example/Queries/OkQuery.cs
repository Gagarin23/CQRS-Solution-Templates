using MediatR;

namespace Application.Example.Queries
{
    public class OkQuery : IRequest<string>
    {
        public string TestMsg { get; set; }
    }
}
