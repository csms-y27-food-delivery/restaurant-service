using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RestaurantService.Presentation.Grpc.Controllers;
using RestaurantService.Presentation.Grpc.Interceptors;

namespace RestaurantService.Presentation.Grpc;

public static class GrpcApplicationExtensions
{
    public static IServiceCollection AddGrpcPresentation(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });
        services.AddGrpcReflection();

        services.AddSingleton<GrpcExceptionInterceptor>();

        return services;
    }

    public static WebApplication MapGrpcPresentation(this WebApplication app)
    {
        app.MapGrpcService<RestaurantValidateController>();
        app.MapGrpcService<RestaurantAdminController>();
        app.MapGrpcService<DishAdminController>();
        app.MapGrpcReflectionService();

        return app;
    }
}