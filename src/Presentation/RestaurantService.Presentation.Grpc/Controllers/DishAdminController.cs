using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RestaurantService.Application.Contracts.Restaurant;
using RestaurantService.Application.Models.Menu;
using RestaurantService.Presentation.Grpc.Mappings;
using RestaurantService.Presentation.Grpc.Protos.Gateway.V1;

namespace RestaurantService.Presentation.Grpc.Controllers;

public sealed class DishAdminController : DishAdminService.DishAdminServiceBase
{
    private readonly IDishManagementService _dishManagementService;

    public DishAdminController(IDishManagementService dishManagementService)
    {
        _dishManagementService = dishManagementService;
    }

    public override async Task<CreateDishResponse> CreateDish(
        CreateDishRequest request,
        ServerCallContext context)
    {
        long dishId = await _dishManagementService.CreateAsync(
            request.RestaurantId,
            request.Name,
            request.Price,
            request.Availability,
            request.Category.ToDomainFoodCategory(),
            context.CancellationToken);

        return new CreateDishResponse
        {
            DishId = dishId,
        };
    }

    public override async Task<GetDishResponse> GetDish(
        GetDishRequest request,
        ServerCallContext context)
    {
        Dish dish = await _dishManagementService.GetByIdAsync(
            request.DishId,
            context.CancellationToken);

        return new GetDishResponse
        {
            Dish = dish.ToGrpcDishInfo(),
        };
    }

    public override async Task<Empty> UpdateDish(
        UpdateDishRequest request,
        ServerCallContext context)
    {
        await _dishManagementService.UpdateAsync(
            request.DishId,
            request.Price,
            request.Availability,
            request.Category.ToDomainFoodCategoryOrNull(),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> DeleteDish(
        DeleteDishRequest request,
        ServerCallContext context)
    {
        await _dishManagementService.DeleteAsync(
            request.DishId,
            context.CancellationToken);

        return new Empty();
    }
}