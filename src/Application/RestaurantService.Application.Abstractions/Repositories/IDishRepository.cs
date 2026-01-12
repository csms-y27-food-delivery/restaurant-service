using RestaurantService.Application.Models.Menu;

namespace RestaurantService.Application.Abstractions.Repositories;

public interface IDishRepository
{
    Task<Dish> GetByIdAsync(long dishId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Dish>> GetByNamesAsync(
        long restaurantId,
        IReadOnlyCollection<string> dishNames,
        CancellationToken cancellationToken);

    Task<long> CreateAsync(
        long restaurantId,
        string dishName,
        long dishPrice,
        bool dishAvailability,
        FoodCategory foodCategory,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        long dishId,
        long? dishPrice,
        bool? dishAvailability,
        FoodCategory? foodCategory,
        CancellationToken cancellationToken);

    Task DeleteAsync(long dishId, CancellationToken cancellationToken);
}