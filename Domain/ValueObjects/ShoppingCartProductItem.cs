namespace Domain.ValueObjects
{
    public record ShoppingCartProductItem(Guid Id, decimal Price, uint Quantity)
    {
        public decimal TotalPrice => Price * Quantity;
    };
}
