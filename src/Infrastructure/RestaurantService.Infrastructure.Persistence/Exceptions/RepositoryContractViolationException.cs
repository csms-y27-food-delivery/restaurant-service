namespace RestaurantService.Infrastructure.Persistence.Exceptions;

public sealed class RepositoryContractViolationException : RepositoryException
{
    public RepositoryContractViolationException(string operation)
        : base(
            code: "repository_contract_violation",
            message: $"Repository contract violation in '{operation}'. No rows returned.",
            operation: operation)
    { }
}