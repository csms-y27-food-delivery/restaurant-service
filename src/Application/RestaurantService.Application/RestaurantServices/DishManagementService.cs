using RestaurantService.Application.Abstractions.Repositories;
using RestaurantService.Application.Contracts.Kafka;
using RestaurantService.Application.Contracts.Restaurant;
using RestaurantService.Application.Models.Menu;

namespace RestaurantService.Application.RestaurantServices;

public sealed class DishManagementService : IDishManagementService
{
    private readonly IDishRepository _dishRepository;
    private readonly IDishEventsProducer _dishEventsProducer;

    public DishManagementService(IDishRepository dishRepository, IDishEventsProducer dishEventsProducer)
    {
        _dishRepository = dishRepository;
        _dishEventsProducer = dishEventsProducer;
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

    public async Task UpdateAsync(
        long dishId,
        long? dishPrice,
        bool? dishAvailability,
        FoodCategory? foodCategory,
        CancellationToken cancellationToken)
    {
        await _dishRepository.UpdateAsync(
            dishId,
            dishPrice,
            dishAvailability,
            foodCategory,
            cancellationToken);

        Dish updatedDish = await _dishRepository.GetByIdAsync(dishId, cancellationToken);
        await _dishEventsProducer.DishUpdateAsync(updatedDish, cancellationToken);
    }

    public Task DeleteAsync(long dishId, CancellationToken cancellationToken)
    {
        return _dishRepository.DeleteAsync(dishId, cancellationToken);
    }
}