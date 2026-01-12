using RestaurantService.Application.Models.Restaurants;

namespace RestaurantService.Application.Abstractions.Repositories;

public interface IRestaurantRepository
{
    Task<Restaurant> GetByIdAsync(long restaurantId, CancellationToken cancellationToken);

    Task<long> CreateAsync(
        string restaurantName,
        string restaurantAddress,
        DeliveryZone restaurantDeliveryZone,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        long restaurantId,
        string? restaurantName,
        string? restaurantAddress,
        DeliveryZone? restaurantDeliveryZone,
        CancellationToken cancellationToken);

    Task DeleteAsync(long restaurantId, CancellationToken cancellationToken);
}