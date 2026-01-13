using Microsoft.Extensions.Options;
using RestaurantService.Application.Contracts.Kafka;
using RestaurantService.Application.Models.Menu;
using RestaurantService.Presentation.Kafka.Options;
using RestaurantService.Presentation.Kafka.Producer;
using RestaurantService.Presentation.Kafka.Protos.Events;

namespace RestaurantService.Presentation.Kafka.Services;

public sealed class DishEventsProducer : IDishEventsProducer
{
    private readonly IKafkaProducer<DishUpdatedKey, DishUpdatedEvent> _producer;
    private readonly IOptions<KafkaTopicsOptions> _topics;

    public DishEventsProducer(
        IKafkaProducer<DishUpdatedKey, DishUpdatedEvent> producer,
        IOptions<KafkaTopicsOptions> topicsOptions)
    {
        _producer = producer;
        _topics = topicsOptions;
    }

    public Task DishUpdateAsync(Dish dish, CancellationToken cancellationToken)
    {
        var key = new DishUpdatedKey
        {
            DishId = dish.DishId,
        };

        var value = new DishUpdatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            DishId = dish.DishId,
            RestaurantId = dish.RestaurantId,
            DishPrice = dish.DishPrice,
            DishAvailability = dish.DishAvailability,
        };

        string topic = _topics.Value.DishUpdatesTopic
                       ?? throw new InvalidOperationException("Kafka topic 'DishUpdatesTopic' is not configured.");

        return _producer.ProduceAsync(topic, key, value, cancellationToken);
    }
}