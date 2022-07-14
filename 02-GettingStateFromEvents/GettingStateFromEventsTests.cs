using Domain.Common;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents;

public class GettingStateFromEventsTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void GettingState_ForSequenceOfEvents_ShouldSucceed()
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

        var shoppingCart = ShoppingCart.Get(events);

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.CustomerId.Should().Be(clientId);
        shoppingCart.Products.Should().HaveCount(2);
        shoppingCart.Products.Should().ContainEquivalentOf(pairOfShoes);
        shoppingCart.Products.Should().ContainEquivalentOf(tShirt);
    }
}
