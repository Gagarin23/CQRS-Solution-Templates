using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Example.Notifications
{
    public class NotificationHandler1 : INotificationHandler<ExampleNotification>
    {
        public async Task Handle(ExampleNotification notification, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            Console.WriteLine("Привет! Это первый обработчик!");
        }
    }
}