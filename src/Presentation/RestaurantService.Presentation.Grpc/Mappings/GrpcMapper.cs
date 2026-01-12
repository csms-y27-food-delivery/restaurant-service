using RestaurantService.Application.Models.Menu;
using RestaurantService.Application.Models.Restaurants;
using RestaurantService.Presentation.Grpc.Protos.Common;
using RestaurantService.Presentation.Grpc.Protos.Order.V1;
using DayOfWeek = RestaurantService.Presentation.Grpc.Protos.Common.DayOfWeek;
using FoodCategory = RestaurantService.Presentation.Grpc.Protos.Common.FoodCategory;

namespace RestaurantService.Presentation.Grpc.Mappings;

internal static class GrpcMapper
{
    public static DishResponse ToGrpcDishResponse(this Dish dish)
    {
        return new DishResponse
        {
            DishId = dish.DishId,
            Name = dish.DishName,
            Price = dish.DishPrice,
            Category = dish.FoodCategory.ToGrpcFoodCategory(),
        };
    }

    public static DishInfo ToGrpcDishInfo(this Dish dish)
    {
        return new DishInfo
        {
            DishId = dish.DishId,
            RestaurantId = dish.RestaurantId,
            Name = dish.DishName,
            Price = dish.DishPrice,
            Availability = dish.DishAvailability,
            Category = dish.FoodCategory.ToGrpcFoodCategory(),
        };
    }

    public static RestaurantInfo ToGrpcRestaurantInfo(this Restaurant restaurant)
    {
        return new RestaurantInfo
        {
            RestaurantId = restaurant.RestaurantId,
            Name = restaurant.RestaurantName,
            Address = restaurant.RestaurantAddress,
            Schedule = restaurant.RestaurantSchedule.ToGrpcWorkSchedule(),
            DeliveryZone = restaurant.RestaurantDeliveryZone.ToGrpcDeliveryZone(),
        };
    }

    public static DeliveryZoneDto ToGrpcDeliveryZone(this DeliveryZone zone)
    {
        return new DeliveryZoneDto
        {
            DeliveryRadiusKm = zone.DeliveryRadius,
            Center = zone.Center.ToGrpcLocation(),
        };
    }

    private static DayOfWeek ToGrpcDay(this System.DayOfWeek day)
    {
        return day switch
        {
            System.DayOfWeek.Sunday => DayOfWeek.Sunday,
            System.DayOfWeek.Monday => DayOfWeek.Monday,
            System.DayOfWeek.Tuesday => DayOfWeek.Tuesday,
            System.DayOfWeek.Wednesday => DayOfWeek.Wednesday,
            System.DayOfWeek.Thursday => DayOfWeek.Thursday,
            System.DayOfWeek.Friday => DayOfWeek.Friday,
            System.DayOfWeek.Saturday => DayOfWeek.Saturday,
            _ => DayOfWeek.Unspecified,
        };
    }

    private static FoodCategory ToGrpcFoodCategory(this RestaurantService.Application.Models.Menu.FoodCategory category)
    {
        return category switch
        {
            RestaurantService.Application.Models.Menu.FoodCategory.Appetizers => FoodCategory.Appetizers,
            RestaurantService.Application.Models.Menu.FoodCategory.MainCourses => FoodCategory.MainCourses,
            RestaurantService.Application.Models.Menu.FoodCategory.Desserts => FoodCategory.Desserts,
            RestaurantService.Application.Models.Menu.FoodCategory.Beverages => FoodCategory.Beverages,
            RestaurantService.Application.Models.Menu.FoodCategory.Sides => FoodCategory.Sides,
            _ => FoodCategory.Unspecified,
        };
    }

    private static Location ToGrpcLocation(this Coordinate coordinate)
    {
        return new Location { Latitude = coordinate.Latitude, Longitude = coordinate.Longitude };
    }

    private static WorkScheduleDto ToGrpcWorkSchedule(this WorkSchedule schedule)
    {
        var dto = new WorkScheduleDto();

        foreach ((System.DayOfWeek day, TimeSlot? slot) in schedule.DailySchedules)
        {
            dto.Days.Add(new DayScheduleDto
            {
                Day = day.ToGrpcDay(),
                OpenMinutes = slot is null ? 0 : (int)slot.OpenTime.TotalMinutes,
                CloseMinutes = slot is null ? 0 : (int)slot.CloseTime.TotalMinutes,
            });
        }

        return dto;
    }
}