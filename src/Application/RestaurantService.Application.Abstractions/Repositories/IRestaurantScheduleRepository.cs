using RestaurantService.Application.Models.Restaurants;

namespace RestaurantService.Application.Abstractions.Repositories;

public interface IRestaurantScheduleRepository
{
    Task ReplaceAsync(long restaurantId, WorkSchedule schedule, CancellationToken cancellationToken);

    Task<int> DeleteByRestaurantIdAsync(long restaurantId, CancellationToken cancellationToken);
}