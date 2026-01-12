using RestaurantService.Application.Abstractions.Repositories;
using RestaurantService.Application.Contracts.Restaurant;
using RestaurantService.Application.Models.Menu;

namespace RestaurantService.Application.RestaurantServices;

public sealed class DishManagementService : IDishManagementService
{
    private readonly IDishRepository _dishRepository;

    public DishManagementService(IDishRepository dishRepository)
    {
        _dishRepository = dishRepository;
    }

    public Task<Dish> GetByIdAsync(long dishId, CancellationToken cancellationToken)
    {
        return _dishRepository.GetByIdAsync(dishId, cancellationToken);
    }

    public Task<long> CreateAsync(
        long restaurantId,
        string dishName,
        long dishPrice,
        bool dishAvailability,
        FoodCategory foodCategory,
        CancellationToken cancellationToken)
    {
        return _dishRepository.CreateAsync(
            restaurantId,
            dishName,
            dishPrice,
            dishAvailability,
            foodCategory,
            cancellationToken);
    }

    public Task UpdateAsync(
        long dishId,
        long? dishPrice,
        bool? dishAvailability,
        FoodCategory? foodCategory,
        CancellationToken cancellationToken)
    {
        return _dishRepository.UpdateAsync(
            dishId,
            dishPrice,
            dishAvailability,
            foodCategory,
            cancellationToken);
    }

    public Task DeleteAsync(long dishId, CancellationToken cancellationToken)
    {
        return _dishRepository.DeleteAsync(dishId, cancellationToken);
    }
}