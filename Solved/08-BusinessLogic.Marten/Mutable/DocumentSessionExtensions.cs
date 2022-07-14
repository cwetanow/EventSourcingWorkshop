using Marten;

namespace IntroductionToEventSourcing.BusinessLogic.Mutable;

public static class DocumentSessionExtensions
{
    public static async Task<TAggregate> Get<TAggregate>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        var entity = await session.Events.AggregateStreamAsync<TAggregate>(id, token: cancellationToken);

        return entity ?? throw new InvalidOperationException($"Entity with id {id} was not found");
    }

    public static Task Add<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TAggregate> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        session.Events.StartStream<TAggregate>(
            getId(command),
            action(command).DequeueUncommittedEvents()
        );

        return session.SaveChangesAsync(cancellationToken);
    }

    public static async Task GetAndUpdate<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Action<TCommand, TAggregate> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : Aggregate
    {
        var id = getId(command);
        var current = await session.Get<TAggregate>(id, cancellationToken);

        action(command, current);

        session.Events.Append(id, current.DequeueUncommittedEvents());

        await session.SaveChangesAsync(cancellationToken);
    }
}
