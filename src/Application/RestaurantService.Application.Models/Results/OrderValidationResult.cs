using RestaurantService.Application.Models.Menu;
using RestaurantService.Application.Models.Restaurants;

namespace RestaurantService.Application.Models.Results;

public record OrderValidationResult(
    bool IsSuccess,
    string? Description,
    DeliveryZone DeliveryZone,
    IReadOnlyList<Dish> Dishes)
{
    public static OrderValidationResult Success(DeliveryZone deliveryZone, IReadOnlyList<Dish> dishes)
    {
        return new OrderValidationResult(true, null, deliveryZone, dishes);
    }

    public static OrderValidationResult Fail(DeliveryZone deliveryZone, string description)
    {
        return new OrderValidationResult(false, description, deliveryZone, []);
    }
}