using RestaurantService.Application.Models.Restaurants;

namespace RestaurantService.Application.Contracts.Restaurant;

public interface IRestaurantManagementService
{
    Task<Models.Restaurants.Restaurant> GetByIdAsync(long restaurantId, CancellationToken cancellationToken);

    Task<long> CreateAsync(
        string restaurantName,
        string restaurantAddress,
        WorkSchedule schedule,
        DeliveryZone deliveryZone,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        long restaurantId,
        string? restaurantName,
        string? restaurantAddress,
        WorkSchedule? schedule,
        DeliveryZone? deliveryZone,
        CancellationToken cancellationToken);

    Task DeleteAsync(long restaurantId, CancellationToken cancellationToken);
}