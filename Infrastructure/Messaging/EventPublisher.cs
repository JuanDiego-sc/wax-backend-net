using Application.Interfaces.Publish;
using MassTransit;

namespace Infrastructure.Messaging;

public class EventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public async Task PublishEventAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : class
    {
        await publishEndpoint.Publish(@event, cancellationToken);
    }
}