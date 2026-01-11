namespace RestaurantService.Application.Models.Menu;

public record RestaurantMenu(long RestaurantId, Dictionary<FoodCategory, Dish[]> Dishes);