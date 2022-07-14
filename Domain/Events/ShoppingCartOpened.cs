using Domain.Common;

namespace Domain.Events
{
    public record ShoppingCartOpened(Guid ShoppingCartId, Guid CustomerId) : DomainEvent;
}
