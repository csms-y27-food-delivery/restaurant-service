namespace RestaurantService.Application.Models.Menu;

public record Dish(
    long DishId,
    string DishName,
    long DishPrice,
    bool DishAvailability,
    long RestaurantId,
    FoodCategory FoodCategory);