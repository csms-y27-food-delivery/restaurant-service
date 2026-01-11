using RestaurantService.Application;
using RestaurantService.Infrastructure.Persistence;
using RestaurantService.Infrastructure.Persistence.BackgroundServices;
using RestaurantService.Presentation.Grpc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddServices()
    .AddPersistence(builder.Configuration)
    .AddMigrations(builder.Configuration)
    .AddGrpcPresentation();

builder.Services.AddHostedService<DatabaseMigrationBackgroundService>();

builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("Infrastructure:Persistence:Postgres"));

WebApplication app = builder.Build();

app.MapGrpcPresentation();

app.Run();