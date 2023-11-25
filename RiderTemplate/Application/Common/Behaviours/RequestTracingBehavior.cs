using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Application.Common.Behaviours
{
    public class RequestTracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource(typeof(TRequest).Name);
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            using (var activity = ActivitySource.StartActivity(ActivitySource.Name))
            {
                try
                {
                    return await next();
                }
                catch (Exception e)
                {
                    if (activity == null)
                    {
                        throw;
                    }
                    
                    activity.SetStatus(ActivityStatusCode.Error, e.ToString());
                    
                    activity.SetTag("payload", JsonSerializer.Serialize(request));
                    
                    throw;
                }
            }
        }
    }
}
