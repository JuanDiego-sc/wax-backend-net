namespace Application.Interfaces.Publish;

public interface IEventPublisher
{
    Task PublishEventAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}