using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RestaurantService.Application.Abstractions.Repositories;
using RestaurantService.Application.Models.Menu;
using RestaurantService.Infrastructure.Persistence.Migrations;
using RestaurantService.Infrastructure.Persistence.Repositories;

namespace RestaurantService.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var pg = new PostgresOptions();
        configuration.GetSection("Infrastructure:Persistence:Postgres").Bind(pg);

        var builder = new NpgsqlDataSourceBuilder(pg.ToString());
        builder.MapEnum<FoodCategory>("food_category");
        NpgsqlDataSource dataSource = builder.Build();

        services.AddSingleton(dataSource);

        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<IRestaurantScheduleRepository, RestaurantScheduleRepository>();
        services.AddScoped<IDishRepository, DishRepository>();

        return services;
    }

    public static IServiceCollection AddMigrations(this IServiceCollection services, IConfiguration configuration)
    {
        var pg = new PostgresOptions();
        configuration.GetSection("Infrastructure:Persistence:Postgres").Bind(pg);

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(pg.ToString())
                .WithMigrationsIn(typeof(IMigrationAssemblyMarker).Assembly))
            .AddLogging(loggingBuilder => loggingBuilder.AddFluentMigratorConsole());

        return services;
    }
}