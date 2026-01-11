using Grpc.Core;
using RestaurantService.Application.Models.Restaurants;
using RestaurantService.Presentation.Grpc.Protos.Common;
using DayOfWeek = RestaurantService.Presentation.Grpc.Protos.Common.DayOfWeek;
using FoodCategory = RestaurantService.Presentation.Grpc.Protos.Common.FoodCategory;

namespace RestaurantService.Presentation.Grpc.Mappings;

internal static class DomainMapper
{
    public static RestaurantService.Application.Models.Menu.FoodCategory ToDomainFoodCategory(this FoodCategory category)
    {
        return category switch
        {
            FoodCategory.Unspecified => throw InvalidArgument("FoodCategory is required."),
            FoodCategory.Appetizers => Application.Models.Menu.FoodCategory.Appetizers,
            FoodCategory.MainCourses => Application.Models.Menu.FoodCategory.MainCourses,
            FoodCategory.Desserts => Application.Models.Menu.FoodCategory.Desserts,
            FoodCategory.Beverages => Application.Models.Menu.FoodCategory.Beverages,
            FoodCategory.Sides => Application.Models.Menu.FoodCategory.Sides,
            _ => throw InvalidArgument("FoodCategory is required."),
        };
    }

    public static RestaurantService.Application.Models.Menu.FoodCategory? ToDomainFoodCategoryOrNull(this FoodCategory category)
    {
        return category switch
        {
            FoodCategory.Unspecified => null,
            FoodCategory.Appetizers => Application.Models.Menu.FoodCategory.Appetizers,
            FoodCategory.MainCourses => Application.Models.Menu.FoodCategory.MainCourses,
            FoodCategory.Desserts => Application.Models.Menu.FoodCategory.Desserts,
            FoodCategory.Beverages => Application.Models.Menu.FoodCategory.Beverages,
            FoodCategory.Sides => Application.Models.Menu.FoodCategory.Sides,
            _ => null,
        };
    }

    public static Coordinate ToDomainCoordinate(this Location location)
    {
        if (location.Latitude < -90 || location.Latitude > 90 || location.Longitude < -180 || location.Longitude > 180)
            throw InvalidArgument("Location is invalid.");
        return new Coordinate(location.Latitude, location.Longitude);
    }

    public static DeliveryZone ToDomainDeliveryZone(this DeliveryZoneDto dto)
    {
        if (dto.DeliveryRadiusKm < 0 || dto.DeliveryRadiusKm > 6371)
            throw InvalidArgument("DeliveryRadiusKm is invalid.");
        return new DeliveryZone(dto.DeliveryRadiusKm, dto.Center.ToDomainCoordinate());
    }

    public static WorkSchedule ToDomainWorkSchedule(this WorkScheduleDto dto)
    {
        var map = new Dictionary<System.DayOfWeek, TimeSlot?>
        {
            [System.DayOfWeek.Sunday] = null,
            [System.DayOfWeek.Monday] = null,
            [System.DayOfWeek.Tuesday] = null,
            [System.DayOfWeek.Wednesday] = null,
            [System.DayOfWeek.Thursday] = null,
            [System.DayOfWeek.Friday] = null,
            [System.DayOfWeek.Saturday] = null,
        };

        foreach (DayScheduleDto day in dto.Days)
        {
            System.DayOfWeek domainDay = day.Day.ToDomainDay();

            map[domainDay] = new TimeSlot(
                FromMinutes(day.OpenMinutes),
                FromMinutes(day.CloseMinutes));
        }

        return new WorkSchedule(map);
    }

    private static System.DayOfWeek ToDomainDay(this DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Unspecified => throw InvalidArgument("DayOfWeek is required."),
            DayOfWeek.Sunday => System.DayOfWeek.Sunday,
            DayOfWeek.Monday => System.DayOfWeek.Monday,
            DayOfWeek.Tuesday => System.DayOfWeek.Tuesday,
            DayOfWeek.Wednesday => System.DayOfWeek.Wednesday,
            DayOfWeek.Thursday => System.DayOfWeek.Thursday,
            DayOfWeek.Friday => System.DayOfWeek.Friday,
            DayOfWeek.Saturday => System.DayOfWeek.Saturday,
            _ => throw InvalidArgument("DayOfWeek is required."),
        };
    }

    private static TimeSpan FromMinutes(int minutes)
    {
        if (minutes < 0 || minutes > 1439)
            throw InvalidArgument("Time must be in range 0..1439 minutes.");

        return TimeSpan.FromMinutes(minutes);
    }

    private static RpcException InvalidArgument(string message)
    {
        return new RpcException(new Status(StatusCode.InvalidArgument, message));
    }
}