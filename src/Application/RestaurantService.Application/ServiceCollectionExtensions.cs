using Microsoft.Extensions.DependencyInjection;
using RestaurantService.Application.Contracts.Restaurant;
using RestaurantService.Application.RestaurantServices;

namespace RestaurantService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IRestaurantValidateService, RestaurantValidateService>();
        services.AddScoped<IRestaurantManagementService, RestaurantManagementService>();
        services.AddScoped<IDishManagementService, DishManagementService>();

        return services;
    }
}