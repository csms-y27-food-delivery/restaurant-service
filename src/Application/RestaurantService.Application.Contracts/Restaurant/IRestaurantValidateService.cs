using RestaurantService.Application.Models.Restaurants;
using RestaurantService.Application.Models.Results;

namespace RestaurantService.Application.Contracts.Restaurant;

public interface IRestaurantValidateService
{
    Task<OrderValidationResult> ValidateOrderAsync(
        long restaurantId,
        IReadOnlyList<string> dishNames,
        Coordinate customerLocation,
        CancellationToken cancellationToken);
}