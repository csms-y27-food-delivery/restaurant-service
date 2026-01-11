namespace RestaurantService.Infrastructure.Persistence.Exceptions;

public sealed class DishNotFoundException : RepositoryException
{
    public DishNotFoundException(long dishId)
        : base(
            code: "dish_not_found",
            message: $"Dish with id='{dishId}' was not found.",
            entityId: dishId,
            operation: "DishRepository.GetByIdAsync")
    { }
}