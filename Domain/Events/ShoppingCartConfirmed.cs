using Domain.Common;

namespace Domain.Events
{
    public record ShoppingCartConfirmed(Guid ShoppingCartId) : DomainEvent;
}
