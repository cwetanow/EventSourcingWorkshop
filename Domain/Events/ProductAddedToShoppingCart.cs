using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Events
{
    public record ProductAddedToShoppingCart(Guid ShoppingCartId, ShoppingCartProductItem Product) : DomainEvent;
}
