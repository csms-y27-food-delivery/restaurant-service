namespace RestaurantService.Infrastructure.Persistence.Exceptions;

public abstract class RepositoryException : Exception
{
    protected RepositoryException(
        string code,
        string message,
        long? entityId = null,
        string? operation = null)
        : base(message)
    {
        Code = code;
        EntityId = entityId;
        Operation = operation;
    }

    public string Code { get; }

    public long? EntityId { get; }

    public string? Operation { get; }
}