using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Example.Queries
{
    public class OkHandler : IRequestHandler<OkQuery, string>
    {
        public async Task<string> Handle(OkQuery request, CancellationToken cancellationToken)
        {
            return request.TestMsg;
        }
    }
}
