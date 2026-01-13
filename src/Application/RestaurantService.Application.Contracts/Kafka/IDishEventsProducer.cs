using RestaurantService.Application.Models.Menu;

namespace RestaurantService.Application.Contracts.Kafka;

public interface IDishEventsProducer
{
    Task DishUpdateAsync(Dish dish, CancellationToken cancellationToken);
}