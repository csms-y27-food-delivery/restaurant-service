namespace RestaurantService.Infrastructure.Persistence.Exceptions;

public sealed class RestaurantNotFoundException : RepositoryException
{
    public RestaurantNotFoundException(long restaurantId)
        : base(
            code: "restaurant_not_found",
            message: $"Restaurant with id='{restaurantId}' was not found.",
            entityId: restaurantId,
            operation: "RestaurantRepository")
    { }
}