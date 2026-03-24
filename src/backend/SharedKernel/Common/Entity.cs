namespace SharedKernel.Common;

public abstract class Entity<TId> where TId : struct
{
    public TId Id { get; protected set; }
}

public abstract class Entity : Entity<int>
{
}