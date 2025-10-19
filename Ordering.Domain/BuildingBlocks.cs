namespace Ordering.Domain
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
    }

    public abstract class AggregateRoot : Entity
    {
        private readonly Queue<object> _domainEvents = new();

        protected void Raise(object @event) => _domainEvents.Enqueue(@event);
        public IEnumerable<object> DequeueDomainEvents()
        {
            while (_domainEvents.TryDequeue(out var e))
                yield return e;
        }
    }
}
