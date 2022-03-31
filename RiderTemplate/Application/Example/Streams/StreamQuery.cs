using MediatR;

namespace Application.Example.Streams
{
    public class StreamQuery : IStreamRequest<int>
    {
        public int Count { get; set; }
    }
}
