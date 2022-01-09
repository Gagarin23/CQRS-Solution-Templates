using System.Collections.Generic;
using MediatR;

namespace Application.Test.Streams
{
    public class StreamQuery : IStreamRequest<int>
    {
        public int Count { get; set; }
    }
}
