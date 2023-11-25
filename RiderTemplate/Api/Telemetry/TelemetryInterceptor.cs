using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;

namespace Api.Telemetry
{
    public class TelemetryInterceptor : IAsyncInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ActivitySource _activitySource;
        private readonly List<TracingInterceptorConfig.Tag> _tags;

        public TelemetryInterceptor(TracingInterceptorConfig config, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _activitySource = new ActivitySource(config.SourceName);
            _tags = config.Tags;
        }

        public void InterceptSynchronous(IInvocation invocation)
        {
            using var activity = _activitySource.StartActivity(invocation.Method?.Name);
            SetTags(activity);
            invocation.Proceed();
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        private async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            using var activity = _activitySource.StartActivity(invocation.Method?.Name);
            SetTags(activity);

            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task;
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            using var activity = _activitySource.StartActivity(invocation.Method?.Name);
            SetTags(activity);

            invocation.Proceed();
            var task = (Task<TResult>)invocation.ReturnValue;
            return await task;
        }

        private void SetTags(Activity activity)
        {
            foreach (var tag in _tags)
            {
                activity?.SetTag(tag.Name, tag.Value);
            }
        }
    }

}
