using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.EventsDefinition;

// 1. Define your events and entity here
public record Product(Guid Id, decimal Price);

public record ShoppingCartOpenedEvent(Guid ShoppingCartId, Guid CustomerId);

public record ProductAddedToShoppingCartEvent(Guid ShoppingCartId, Guid CustomerId, Product Product, uint Quantity);
public record ProductRemovedFromShoppingCartEvent(Guid ShoppingCartId, Guid CustomerId, Guid ProductId);

public record ShoppingCartConfirmedEvent(Guid ShoppingCartId, Guid CustomerId);
public record ShoppingCartCancelledEvent(Guid ShoppingCartId, Guid CustomerId);

public class EventsDefinitionTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void AllEventTypes_ShouldBeDefined()
    {
        var shoppingCardId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var product = new Product(Guid.NewGuid(), 7.66m);

        var events = new object[]
        {
            // 2. Put your sample events here
            new ShoppingCartOpenedEvent(shoppingCardId,customerId),
            new ProductAddedToShoppingCartEvent(shoppingCardId,customerId,product,4),
            new ProductRemovedFromShoppingCartEvent(shoppingCardId,customerId,product.Id),
            new ShoppingCartConfirmedEvent(shoppingCardId,customerId),
            new ShoppingCartCancelledEvent(shoppingCardId,customerId),
        };

        const int expectedEventTypesCount = 5;
        events.Should().HaveCount(expectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCount(expectedEventTypesCount);
    }
}
