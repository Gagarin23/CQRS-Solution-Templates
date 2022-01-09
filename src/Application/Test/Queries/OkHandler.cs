using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Application.Test.Queries
{
    public class OkHandler : IRequestHandler<OkQuery, string>
    {
        public async Task<string> Handle(OkQuery request, CancellationToken cancellationToken)
        {
            return request.TestMsg;
        }
    }
}
