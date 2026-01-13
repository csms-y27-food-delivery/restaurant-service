using RestaurantService.Application;
using RestaurantService.Infrastructure.Persistence;
using RestaurantService.Infrastructure.Persistence.BackgroundServices;
using RestaurantService.Presentation.Grpc;
using RestaurantService.Presentation.Kafka;
using RestaurantService.Presentation.Kafka.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddServices()
    .AddPersistence(builder.Configuration)
    .AddMigrations(builder.Configuration)
    .AddGrpcPresentation()
    .AddKafka();

builder.Services.AddHostedService<DatabaseMigrationBackgroundService>();

builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("Infrastructure:Persistence:Postgres"));
builder.Services.Configure<KafkaProducerOptions>(builder.Configuration.GetSection("Kafka:Producer"));
builder.Services.Configure<KafkaTopicsOptions>(builder.Configuration.GetSection("Kafka:Topics"));

WebApplication app = builder.Build();

app.MapGrpcPresentation();

app.Run();