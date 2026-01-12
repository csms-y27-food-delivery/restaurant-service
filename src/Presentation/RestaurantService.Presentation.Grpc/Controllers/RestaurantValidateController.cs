using Grpc.Core;
using RestaurantService.Application.Contracts.Restaurant;
using RestaurantService.Application.Models.Results;
using RestaurantService.Presentation.Grpc.Mappings;
using RestaurantService.Presentation.Grpc.Protos.Order.V1;

namespace RestaurantService.Presentation.Grpc.Controllers;

public sealed class RestaurantValidateController : RestaurantValidateService.RestaurantValidateServiceBase
{
    private readonly IRestaurantValidateService _restaurantValidateService;

    public RestaurantValidateController(IRestaurantValidateService restaurantValidateService)
    {
        _restaurantValidateService = restaurantValidateService;
    }

    public override async Task<ValidateOrderResponse> ValidateOrder(
        ValidateOrderRequest request,
        ServerCallContext context)
    {
        OrderValidationResult result = await _restaurantValidateService.ValidateOrderAsync(
            request.RestaurantId,
            request.DishNames,
            request.CustomerLocation.ToDomainCoordinate(),
            context.CancellationToken);

        var response = new ValidateOrderResponse
        {
            IsSuccess = result.IsSuccess,
            DeliveryZone = result.DeliveryZone.ToGrpcDeliveryZone(),
        };

        if (result.Description is not null)
        {
            response.Description = result.Description;
        }

        response.Dishes.AddRange(result.Dishes.Select(dish => dish.ToGrpcDishResponse()));

        return response;
    }
}