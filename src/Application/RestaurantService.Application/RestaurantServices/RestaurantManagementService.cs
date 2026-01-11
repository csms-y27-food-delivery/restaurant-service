using RestaurantService.Application.Abstractions.Repositories;
using RestaurantService.Application.Contracts.Restaurant;
using RestaurantService.Application.Models.Restaurants;
using System.Transactions;

namespace RestaurantService.Application.RestaurantServices;

public sealed class RestaurantManagementService : IRestaurantManagementService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IRestaurantScheduleRepository _scheduleRepository;

    public RestaurantManagementService(
        IRestaurantRepository restaurantRepository,
        IRestaurantScheduleRepository scheduleRepository)
    {
        _restaurantRepository = restaurantRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Restaurant> GetByIdAsync(long restaurantId, CancellationToken cancellationToken)
    {
        return await _restaurantRepository.GetByIdAsync(restaurantId, cancellationToken);
    }

    public async Task<long> CreateAsync(
        string restaurantName,
        string restaurantAddress,
        WorkSchedule schedule,
        DeliveryZone deliveryZone,
        CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        long restaurantId = await _restaurantRepository.CreateAsync(
            restaurantName,
            restaurantAddress,
            deliveryZone,
            cancellationToken);

        await _scheduleRepository.ReplaceAsync(restaurantId, schedule, cancellationToken);

        scope.Complete();
        return restaurantId;
    }

    public async Task UpdateAsync(
        long restaurantId,
        string? restaurantName,
        string? restaurantAddress,
        WorkSchedule? schedule,
        DeliveryZone? deliveryZone,
        CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _restaurantRepository.UpdateAsync(
            restaurantId,
            restaurantName,
            restaurantAddress,
            deliveryZone,
            cancellationToken);

        if (schedule is not null)
            await _scheduleRepository.ReplaceAsync(restaurantId, schedule, cancellationToken);

        scope.Complete();
    }

    public async Task DeleteAsync(long restaurantId, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _scheduleRepository.DeleteByRestaurantIdAsync(restaurantId, cancellationToken);

        await _restaurantRepository.DeleteAsync(restaurantId, cancellationToken);

        scope.Complete();
    }
}
