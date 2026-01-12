using RestaurantService.Application.Abstractions.Repositories;
using RestaurantService.Application.Contracts.Restaurant;
using RestaurantService.Application.Helpers;
using RestaurantService.Application.Models.Menu;
using RestaurantService.Application.Models.Restaurants;
using RestaurantService.Application.Models.Results;

namespace RestaurantService.Application.RestaurantServices;

public sealed class RestaurantValidateService : IRestaurantValidateService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IDishRepository _dishRepository;
    private readonly TimeProvider _timeProvider;

    public RestaurantValidateService(
        IRestaurantRepository restaurantRepository,
        IDishRepository dishRepository,
        TimeProvider timeProvider)
    {
        _restaurantRepository = restaurantRepository;
        _dishRepository = dishRepository;
        _timeProvider = timeProvider;
    }

    public async Task<OrderValidationResult> ValidateOrderAsync(
        long restaurantId,
        IReadOnlyList<string> dishNames,
        Coordinate customerLocation,
        CancellationToken cancellationToken)
    {
        Restaurant restaurant = await _restaurantRepository.GetByIdAsync(restaurantId, cancellationToken);
        DateTimeOffset now = _timeProvider.GetLocalNow();

        if (dishNames.Count == 0)
            return OrderValidationResult.Fail(restaurant.RestaurantDeliveryZone, "Order has no dishes.");

        if (!RestaurantRules.IsRestaurantOpen(restaurant.RestaurantSchedule, now))
            return OrderValidationResult.Fail(restaurant.RestaurantDeliveryZone, "Restaurant is closed.");

        if (!RestaurantRules.IsDeliveryAvailable(customerLocation, restaurant.RestaurantDeliveryZone))
            return OrderValidationResult.Fail(restaurant.RestaurantDeliveryZone, "Delivery is not available for this location.");

        string[] normalizedRequested = dishNames.Select(DishName.Normalize).ToArray();

        string[] distinctRequested = normalizedRequested
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        IReadOnlyList<Dish> found = await _dishRepository.GetByNamesAsync(
            restaurantId,
            distinctRequested,
            cancellationToken);

        var dishByName = found.ToDictionary(
            dish => DishName.Normalize(dish.DishName),
            dish => dish,
            StringComparer.Ordinal);

        string[] missing = distinctRequested
            .Where(name => !dishByName.ContainsKey(name))
            .ToArray();

        if (missing.Length > 0)
        {
            return OrderValidationResult.Fail(restaurant.RestaurantDeliveryZone, "Some dishes do not exist for this restaurant: " + string.Join(", ", missing));
        }

        string[] unavailable = dishByName.Values
            .Where(dish => !dish.DishAvailability)
            .Select(dish => dish.DishName)
            .ToArray();

        if (unavailable.Length > 0)
        {
            return OrderValidationResult.Fail(restaurant.RestaurantDeliveryZone, "Some dishes are not available: " + string.Join(", ", unavailable));
        }

        var ordered = normalizedRequested
            .Select(name => dishByName[name])
            .ToList();

        return OrderValidationResult.Success(restaurant.RestaurantDeliveryZone, ordered);
    }
}