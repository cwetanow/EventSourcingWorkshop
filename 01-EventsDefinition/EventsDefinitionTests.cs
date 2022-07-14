using Domain.Events;
using Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.EventsDefinition;

// 1. Define your events and entity here

public class EventsDefinitionTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void AllEventTypes_ShouldBeDefined()
    {
        var shoppingCardId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var product = new ProductItem(Guid.NewGuid(), 7.66m);

        var events = new object[]
        {
            // 2. Put your sample events here
            new ShoppingCartOpened(shoppingCardId,customerId),
            new ProductAddedToShoppingCart(shoppingCardId,product,4),
            new ProductRemovedFromShoppingCart(shoppingCardId,product.Id),
            new ShoppingCartConfirmed(shoppingCardId),
            new ShoppingCartCancelled(shoppingCardId),
        };

        const int expectedEventTypesCount = 5;
        events.Should().HaveCount(expectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCount(expectedEventTypesCount);
    }
}
