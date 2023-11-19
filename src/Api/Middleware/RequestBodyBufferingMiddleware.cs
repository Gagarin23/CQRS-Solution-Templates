using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using Api.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Api.Middleware
{
    public class RequestBodyBufferingMiddleware
    {
        public const string RequestBodyKey = "RequestBodyBytes";

        private readonly RequestDelegate _next;

        public RequestBodyBufferingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOptionsMonitor<TracingConfig> monitor)
        {
            var config = monitor.CurrentValue;

            if (!config.CollectHttpBodyEnabled)
            {
                await _next(context);
                return;
            }

            var httpBodySizeLimitInBytes = config.HttpBodySizeLimitInBytes;

            using var ms = new MemoryStream();

            context.Request.EnableBuffering();

            var buffer = ArrayPool<byte>.Shared.Rent(httpBodySizeLimitInBytes);
            try
            {
                while (true)
                {
                    var bytesRead = await context.Request.Body.ReadAsync(new Memory<byte>(buffer), context.RequestAborted);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    var bytesLengthToWrite = Math.Min(bytesRead, httpBodySizeLimitInBytes - (int)ms.Length);
                    await ms.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesLengthToWrite), context.RequestAborted);

                    if (ms.Length >= httpBodySizeLimitInBytes)
                    {
                        break;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
                context.Request.Body.Position = 0;
            }

            if (ms.Length > 0)
            {
                context.Items.Add(RequestBodyKey, ms.ToArray());
            }

            await _next(context);
        }
    }
}
