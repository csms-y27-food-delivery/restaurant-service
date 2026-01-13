namespace RestaurantService.Presentation.Kafka.Options;

public class KafkaProducerOptions
{
    public string? BootstrapServers { get; set; }

    public string? ClientId { get; set; }
}