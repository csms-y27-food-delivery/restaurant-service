using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RestaurantService.Application.Contracts.Restaurant;
using RestaurantService.Application.Models.Restaurants;
using RestaurantService.Presentation.Grpc.Mappings;
using RestaurantService.Presentation.Grpc.Protos.Gateway;

namespace RestaurantService.Presentation.Grpc.Controllers;

public sealed class RestaurantAdminController : RestaurantAdminService.RestaurantAdminServiceBase
{
    private readonly IRestaurantManagementService _restaurantManagementService;

    public RestaurantAdminController(IRestaurantManagementService restaurantManagementService)
    {
        _restaurantManagementService = restaurantManagementService;
    }

    public override async Task<CreateRestaurantResponse> CreateRestaurant(
        CreateRestaurantRequest request,
        ServerCallContext context)
    {
        long restaurantId = await _restaurantManagementService.CreateAsync(
            request.Name,
            request.Address,
            request.Schedule.ToDomainWorkSchedule(),
            request.DeliveryZone.ToDomainDeliveryZone(),
            context.CancellationToken);

        return new CreateRestaurantResponse
        {
            RestaurantId = restaurantId,
        };
    }

    public override async Task<GetRestaurantResponse> GetRestaurant(
        GetRestaurantRequest request,
        ServerCallContext context)
    {
        Restaurant restaurant = await _restaurantManagementService.GetByIdAsync(
            request.RestaurantId,
            context.CancellationToken);

        return new GetRestaurantResponse
        {
            Restaurant = restaurant.ToGrpcRestaurantInfo(),
        };
    }

    public override async Task<Empty> UpdateRestaurant(
        UpdateRestaurantRequest request,
        ServerCallContext context)
    {
        await _restaurantManagementService.UpdateAsync(
            request.RestaurantId,
            request.Name,
            request.Address,
            request.Schedule?.ToDomainWorkSchedule(),
            request.DeliveryZone?.ToDomainDeliveryZone(),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> DeleteRestaurant(
        DeleteRestaurantRequest request,
        ServerCallContext context)
    {
        await _restaurantManagementService.DeleteAsync(
            request.RestaurantId,
            context.CancellationToken);

        return new Empty();
    }
}