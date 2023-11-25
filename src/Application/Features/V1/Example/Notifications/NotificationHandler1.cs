using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Application.Features.V1.Example.Notifications
{
    public class NotificationHandler1 : INotificationHandler<ExampleNotification>
    {
        public async Task Handle(ExampleNotification notification, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            Console.WriteLine("Good afternoon sir or lady. This is the number one handler.");
        }
    }
}
