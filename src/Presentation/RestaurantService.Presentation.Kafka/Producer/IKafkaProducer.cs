namespace RestaurantService.Presentation.Kafka.Producer;

public interface IKafkaProducer<TKey, TValue>
{
    Task ProduceAsync(string topic, TKey key, TValue value,  CancellationToken cancellationToken);
}