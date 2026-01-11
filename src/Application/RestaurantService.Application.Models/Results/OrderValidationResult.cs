using RestaurantService.Application.Models.Menu;

namespace RestaurantService.Application.Models.Results;

public record OrderValidationResult(
    bool IsSuccess,
    string? Description,
    IReadOnlyList<Dish> Dishes)
{
    public static OrderValidationResult Success(IReadOnlyList<Dish> dishes)
    {
        return new OrderValidationResult(true, null, dishes);
    }

    public static OrderValidationResult Fail(string description)
    {
        return new OrderValidationResult(false, description, []);
    }
}