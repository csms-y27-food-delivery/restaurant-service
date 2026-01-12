using RestaurantService.Application.Models.Restaurants;

namespace RestaurantService.Application.Helpers;

internal static class RestaurantRules
{
    public static bool IsRestaurantOpen(WorkSchedule schedule, DateTimeOffset now)
    {
        DayOfWeek day = now.DayOfWeek;
        TimeSpan currentTime = now.TimeOfDay;

        if (!schedule.DailySchedules.TryGetValue(day, out TimeSlot? slot) || slot is null)
            return false;

        return slot.OpenTime < slot.CloseTime
            ? currentTime >= slot.OpenTime && currentTime < slot.CloseTime
            : currentTime >= slot.OpenTime || currentTime < slot.CloseTime;
    }

    public static bool IsDeliveryAvailable(Coordinate customer, DeliveryZone zone)
    {
        double distanceKm = GeoDistance.HaversineKm(zone.Center, customer);
        return distanceKm <= zone.DeliveryRadius;
    }
}