namespace RestaurantService.Application.Models.Restaurants;

public record Restaurant(
    long RestaurantId,
    string RestaurantName,
    string RestaurantAddress,
    WorkSchedule RestaurantSchedule,
    DeliveryZone RestaurantDeliveryZone);