using Domain.Common;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using FluentAssertions;
using IntroductionToEventSourcing.GettingStateFromEvents.Tools;
using Marten;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Immutable;

public class GettingStateFromEventsTests : MartenTest
{
    /// <summary>
    /// Solution - Immutable entity
    /// </summary>
    /// <param name="documentSession"></param>
    /// <param name="shoppingCartId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<ShoppingCart> GetShoppingCart(IDocumentSession documentSession, Guid shoppingCartId,
        CancellationToken cancellationToken)
    {
        var streamEvents = await documentSession.Events.FetchStreamAsync(shoppingCartId, token: cancellationToken);
        var events = streamEvents
            .Select(ev => ev.Data as DomainEvent);

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

        await AppendEvents(shoppingCartId, events, CancellationToken.None);

        var shoppingCart = await GetShoppingCart(DocumentSession, shoppingCartId, CancellationToken.None);

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
