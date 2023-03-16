using MediatR;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Example.Streams
{
    public class StreamHandler : IStreamRequestHandler<StreamQuery, int>
    {
        public async IAsyncEnumerable<int> Handle(StreamQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < request.Count; i++)
            {
                await Task.Delay(500, cancellationToken);
                yield return i;
            }
        }
    }
}
