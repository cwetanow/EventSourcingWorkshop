using Domain.Common;

namespace Domain.Events
{
    public record ProductRemovedFromShoppingCart(Guid ShoppingCartId, Guid ProductId) : DomainEvent;
}
