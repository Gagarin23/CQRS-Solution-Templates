using MediatR;

namespace Application.Features.V1.Example.Streams
{
    public class StreamQuery : IStreamRequest<int>
    {
        public int Count { get; set; }
    }
}
