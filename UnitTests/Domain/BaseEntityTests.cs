using Domain.Entities;
using Domain.Events;

namespace UnitTests.Domain;

// Concrete subclass for testing abstract BaseEntity
file class TestEntity : BaseEntity { }

// Minimal IDomainEvent implementation for testing
file class TestDomainEvent : IDomainEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public class BaseEntityTests
{
    [Fact]
    public void Id_DefaultsToNewGuid()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(entity.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void Id_IsDifferentForEachInstance()
    {
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Fact]
    public void CreatedAt_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var entity = new TestEntity();
        var after = DateTime.UtcNow.AddSeconds(1);

        entity.CreatedAt.Should().BeAfter(before);
        entity.CreatedAt.Should().BeBefore(after);
    }

    [Fact]
    public void UpdatedAt_DefaultsToNull()
    {
        var entity = new TestEntity();

        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void AddDomainEvent_AddsEventToCollection()
    {
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        entity.AddDomainEvent(domainEvent);

        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_MultipleEvents_AddsAll()
    {
        var entity = new TestEntity();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        entity.AddDomainEvent(event1);
        entity.AddDomainEvent(event2);

        entity.DomainEvents.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveDomainEvent_RemovesEventFromCollection()
    {
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        entity.AddDomainEvent(domainEvent);
        entity.RemoveDomainEvent(domainEvent);

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_IsReadOnly()
    {
        var entity = new TestEntity();

        var domainEvents = entity.DomainEvents;

        domainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }
}
