using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using RestaurantService.Application.Contracts.Kafka;
using RestaurantService.Presentation.Kafka.Producer;
using RestaurantService.Presentation.Kafka.Protos.Events;
using RestaurantService.Presentation.Kafka.Services;

namespace RestaurantService.Presentation.Kafka;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection services)
    {
        services.AddSingleton<ISerializer<DishUpdatedKey>, ProtobufSerializer<DishUpdatedKey>>();
        services.AddSingleton<ISerializer<DishUpdatedEvent>, ProtobufSerializer<DishUpdatedEvent>>();

        services.AddSingleton<IKafkaProducer<DishUpdatedKey, DishUpdatedEvent>, KafkaProducer<DishUpdatedKey, DishUpdatedEvent>>();

        services.AddSingleton<IDishEventsProducer, DishEventsProducer>();

        return services;
    }
}