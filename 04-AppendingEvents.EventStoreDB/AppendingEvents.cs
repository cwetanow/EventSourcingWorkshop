using Domain.Common;
using Domain.Events;
using Domain.ValueObjects;
using EventStore.Client;
using FluentAssertions;
using Newtonsoft.Json;
using System.Text;
using Xunit;

namespace IntroductionToEventSourcing.AppendingEvents;

public class GettingStateFromEventsTests
{
    private async Task<IWriteResult> AppendEvents(EventStoreClient eventStore, string streamName, object[] events,
        CancellationToken ct)
    {
        // TODO: Fill append events logic here.
        var eventData = events
            .Select(ev => ev as DomainEvent)
            .Select(ev => new EventData(Uuid.FromGuid(ev.Id), ev.GetType().Name, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ev))));

        return await eventStore.AppendToStreamAsync(streamName, StreamState.Any, eventData, cancellationToken: ct);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public async Task GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();
        var twoPairsOfShoes = new ShoppingCartProductItem(shoesId, 100, 2);
        var pairOfShoes = new ShoppingCartProductItem(shoesId, 100, 1);
        var tShirt = new ShoppingCartProductItem(tShirtId, 50, 1);

        var events = new DomainEvent[]
        {
            new ShoppingCartOpened(shoppingCartId, clientId),
            new ProductAddedToShoppingCart(shoppingCartId, twoPairsOfShoes),
            new ProductAddedToShoppingCart(shoppingCartId, tShirt),
            new ProductRemovedFromShoppingCart(shoppingCartId, pairOfShoes),
            new ShoppingCartConfirmed(shoppingCartId),
            new ShoppingCartCancelled(shoppingCartId)
        };

        await using var eventStore =
            new EventStoreClient(EventStoreClientSettings.Create("esdb://localhost:2113?tls=false"));

        var streamName = $"shopping_cart-{shoppingCartId}";

        var appendedEvents = 0ul;
        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await AppendEvents(eventStore, streamName, events, CancellationToken.None);
            appendedEvents = result.NextExpectedStreamRevision;
        });

        exception.Should().BeNull();
        appendedEvents.Should().Be((ulong)events.Length - 1);
    }
}
