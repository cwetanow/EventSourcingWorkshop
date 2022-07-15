using System.Text;
using Domain.Common;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using EventStore.Client;
using FluentAssertions;
using IntroductionToEventSourcing.GettingStateFromEvents.Tools;
using Newtonsoft.Json;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Immutable;

public class GettingStateFromEventsTests : EventStoreDBTest
{
    /// <summary>
    /// </summary>
    /// <returns></returns>
    private static async Task<ShoppingCart> GetShoppingCart(EventStoreClient eventStore, string streamName, CancellationToken ct)
    {
        // 1. Add logic here
        var events = await eventStore.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, cancellationToken: ct)
             .Select(ev =>
             (JsonConvert.DeserializeObject(Encoding.UTF8.GetString(ev.Event.Data.Span),
             typeof(DomainEvent).Assembly.GetType(ev.Event.EventType)) as DomainEvent)!)
             .ToListAsync(ct);

        return ShoppingCart.Get(events);
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

        var streamName = $"shopping_cart-{shoppingCartId}";

        await AppendEvents(streamName, events, CancellationToken.None);

        var shoppingCart = await GetShoppingCart(EventStore, streamName, CancellationToken.None);
        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.CustomerId.Should().Be(clientId);
        shoppingCart.Products.Should().HaveCount(2);

        shoppingCart.Products[0].Id.Should().Be(shoesId);
        shoppingCart.Products[0].Quantity.Should().Be(pairOfShoes.Quantity);
        shoppingCart.Products[0].Price.Should().Be(pairOfShoes.Price);

        shoppingCart.Products[1].Id.Should().Be(tShirtId);
        shoppingCart.Products[1].Quantity.Should().Be(tShirt.Quantity);
        shoppingCart.Products[1].Price.Should().Be(tShirt.Price);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Cancelled);
    }
}
