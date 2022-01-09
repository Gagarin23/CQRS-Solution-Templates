using MediatR;

namespace Application.Test.Queries
{
    public class OkQuery : IRequest<string>
    {
        public string TestMsg { get; set; }
    }
}
