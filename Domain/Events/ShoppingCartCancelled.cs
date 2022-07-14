using Domain.Common;

namespace Domain.Events
{
    public record ShoppingCartCancelled(Guid ShoppingCartId) : DomainEvent;
}
