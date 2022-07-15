using Domain.Common;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public enum ShoppingCartStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 4
    }

    public class ShoppingCart : Entity
    {
        private ShoppingCart()
        { }

        public Guid CustomerId { get; private set; }
        public IList<ShoppingCartProductItem> Products { get; } = new List<ShoppingCartProductItem>();
        public ShoppingCartStatus Status { get; private set; }

        public void When(DomainEvent @event)
        {
            switch (@event)
            {
                case ShoppingCartOpened opened:
                    Aplly(opened);
                    break;
                case ProductAddedToShoppingCart added:
                    Aplly(added);
                    break;
                case ProductRemovedFromShoppingCart removed:
                    Aplly(removed);
                    break;
                case ShoppingCartConfirmed confirmed:
                    Aplly(confirmed);
                    break;
                case ShoppingCartCancelled cancelled:
                    Aplly(cancelled);
                    break;
                default:
                    break;
            }
        }

        private void Aplly(ShoppingCartCancelled _)
        {
            Status = ShoppingCartStatus.Cancelled;
        }

        private void Aplly(ShoppingCartConfirmed _)
        {
            Status = ShoppingCartStatus.Confirmed;
        }

        private void Aplly(ProductRemovedFromShoppingCart evt)
        {
            var product = Products.SingleOrDefault(p => p.Id == evt.Product.Id);
            if (product is null)
            {
                return;
            }

            var index = Products.IndexOf(product);
            Products.Remove(product);
            
            var productWithUpdatedQuantity = new ShoppingCartProductItem(product.Id, product.Price, product.Quantity - evt.Product.Quantity);
            Products.Insert(index, productWithUpdatedQuantity);
        }

        private void Aplly(ProductAddedToShoppingCart evt)
        {
            Products.Add(evt.Product);
        }

        private void Aplly(ShoppingCartOpened evt)
        {
            this.CustomerId = evt.CustomerId;
            this.Id = evt.ShoppingCartId;
            this.Status = ShoppingCartStatus.Pending;
        }

        public static ShoppingCart Get(IEnumerable<DomainEvent> events)
        {
            var cart = new ShoppingCart();

            foreach (var ev in events)
            {
                cart.When(ev);
            }

            return cart;
        }
    }
}
