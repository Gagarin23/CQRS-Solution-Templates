using System;
using Application.Common.Constants;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public class RequestContext : IRequestContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? _username;
        public Guid RequestId { get; } = Guid.NewGuid();
        public string? Username => _username ??= GetUsernameOrDefault();

        public RequestContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool TryGetUsername(out string? username)
        {
            username = null;
            try
            {
                if (Username != null)
                {
                    username = Username;
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private string? GetUsernameOrDefault()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            ThrowIfHttpContextIsNull(httpContext);

            _username = httpContext.GetUsername();
            return _username;
        }

        private void ThrowIfHttpContextIsNull(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new NotSupportedException(ExceptionMessages.HttpContextIsMissing);
            }
        }
    }
}


