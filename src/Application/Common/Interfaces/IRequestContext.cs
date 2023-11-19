using System;

namespace Application.Common.Interfaces
{
    public interface IRequestContext
    {
        public Guid RequestId { get; }

        string? Username { get; }
    }
}
