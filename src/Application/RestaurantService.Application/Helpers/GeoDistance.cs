using RestaurantService.Application.Models.Restaurants;

namespace RestaurantService.Application.Helpers;

internal static class GeoDistance
{
    private const double EarthRadiusKm = 6371.0;

    public static double HaversineKm(Coordinate a, Coordinate b)
    {
        double dLat = ToRadians(b.Latitude - a.Latitude);
        double dLon = ToRadians(b.Longitude - a.Longitude);

        double lat1 = ToRadians(a.Latitude);
        double lat2 = ToRadians(b.Latitude);

        double sinLat = Math.Sin(dLat / 2);
        double sinLon = Math.Sin(dLon / 2);

        double h = (sinLat * sinLat) + (Math.Cos(lat1) * Math.Cos(lat2) * sinLon * sinLon);
        double c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));

        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}