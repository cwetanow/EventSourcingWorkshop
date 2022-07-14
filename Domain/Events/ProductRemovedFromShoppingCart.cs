using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Events
{
    public record ProductRemovedFromShoppingCart(Guid ShoppingCartId, ShoppingCartProductItem Product) : DomainEvent;
}
