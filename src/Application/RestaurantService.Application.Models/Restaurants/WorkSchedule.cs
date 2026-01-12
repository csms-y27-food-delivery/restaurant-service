namespace RestaurantService.Application.Models.Restaurants;

public record WorkSchedule(Dictionary<DayOfWeek, TimeSlot?> DailySchedules);