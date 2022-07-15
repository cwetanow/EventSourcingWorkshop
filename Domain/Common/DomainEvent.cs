namespace Domain.Common
{
    public record DomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
}
