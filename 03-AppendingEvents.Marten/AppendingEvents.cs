using Domain.Common;
using Domain.Events;
using Domain.ValueObjects;
using FluentAssertions;
using IntroductionToEventSourcing.AppendingEvents.Tools;
using Marten;
using Xunit;

namespace IntroductionToEventSourcing.AppendingEvents;

public class GettingStateFromEventsTests
{
    private static async Task AppendEvents(IDocumentSession documentSession, Guid shoppingCartId, object[] events, CancellationToken ct)
    {
        // TODO: Fill append events logic here.
        documentSession.Events.Append(shoppingCartId, events);
        await documentSession.SaveChangesAsync(ct);
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

        const string connectionString =
            "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'";

        using var documentStore = DocumentStore.For(connectionString);
        await using var documentSession = documentStore.LightweightSession();

        documentSession.Listeners.Add(MartenEventsChangesListener.Instance);

        var exception = await Record.ExceptionAsync(async () =>
            await AppendEvents(documentSession, shoppingCartId, events, CancellationToken.None)
        );
        exception.Should().BeNull();
        MartenEventsChangesListener.Instance.AppendedEventsCount.Should().Be(events.Length);
    }
}
