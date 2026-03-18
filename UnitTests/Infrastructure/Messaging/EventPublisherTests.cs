using Infrastructure.Messaging;
using MassTransit;
using Moq;

namespace UnitTests.Infrastructure.Messaging;

public class EventPublisherTests
{
    private class TestEvent
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public async Task PublishEventAsync_CallsPublishEndpointWithEvent()
    {
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var publisher = new EventPublisher(publishEndpointMock.Object);
        var testEvent = new TestEvent { Name = "Test", Value = 42 };

        await publisher.PublishEventAsync(testEvent, CancellationToken.None);

        publishEndpointMock.Verify(
            p => p.Publish(testEvent, CancellationToken.None),
            Times.Once
        );
    }

    [Fact]
    public async Task PublishEventAsync_WithDefaultCancellationToken_UsesDefaultToken()
    {
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var publisher = new EventPublisher(publishEndpointMock.Object);
        var testEvent = new TestEvent { Name = "Test", Value = 42 };

        await publisher.PublishEventAsync(testEvent);

        publishEndpointMock.Verify(
            p => p.Publish(testEvent, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task PublishEventAsync_PassesCancellationTokenToEndpoint()
    {
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var publisher = new EventPublisher(publishEndpointMock.Object);
        var testEvent = new TestEvent { Name = "Test", Value = 42 };
        var cancellationToken = new CancellationToken();

        await publisher.PublishEventAsync(testEvent, cancellationToken);

        publishEndpointMock.Verify(
            p => p.Publish(testEvent, cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task PublishEventAsync_WithDifferentEventTypes_PublishesCorrectType()
    {
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var publisher = new EventPublisher(publishEndpointMock.Object);
        var event1 = new TestEvent { Name = "First", Value = 1 };
        var event2 = new TestEvent { Name = "Second", Value = 2 };

        await publisher.PublishEventAsync(event1);
        await publisher.PublishEventAsync(event2);

        publishEndpointMock.Verify(
            p => p.Publish(event1, It.IsAny<CancellationToken>()),
            Times.Once
        );
        publishEndpointMock.Verify(
            p => p.Publish(event2, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
