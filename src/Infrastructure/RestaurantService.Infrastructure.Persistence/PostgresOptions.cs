namespace RestaurantService.Infrastructure.Persistence;

public sealed class PostgresOptions
{
    public string Host { get; init; } = string.Empty;

    public string Database { get; init; } = string.Empty;

    public int Port { get; init; } = 5432;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public override string ToString()
    {
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
    }
}
