namespace RestaurantService.Application.Helpers;

internal static class DishName
{
    public static string Normalize(string name)
    {
        return name.Trim().ToLowerInvariant();
    }
}