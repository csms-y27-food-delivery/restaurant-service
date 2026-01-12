using Npgsql;
using RestaurantService.Application.Abstractions.Repositories;
using RestaurantService.Application.Models.Restaurants;
using RestaurantService.Infrastructure.Persistence.Exceptions;

namespace RestaurantService.Infrastructure.Persistence.Repositories;

public sealed class RestaurantRepository : IRestaurantRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public RestaurantRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Restaurant> GetByIdAsync(long restaurantId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select
                               r.restaurant_id,
                               r.restaurant_name,
                               r.restaurant_address,
                               r.delivery_radius_km,
                               r.delivery_center_lat,
                               r.delivery_center_lon,
                               wh.day_of_week,
                               wh.open_time,
                               wh.close_time
                           from restaurants r
                           left join restaurant_working_hours wh on wh.restaurant_id = r.restaurant_id
                           where r.restaurant_id = :restaurant_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters = { new NpgsqlParameter("restaurant_id", restaurantId) },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new RestaurantNotFoundException(restaurantId);

        long id = reader.GetInt64(0);
        string name = reader.GetString(1);
        string address = reader.GetString(2);
        double radius = reader.GetDouble(3);
        double lat = reader.GetDouble(4);
        double lon = reader.GetDouble(5);

        var schedule = new Dictionary<DayOfWeek, TimeSlot?>
        {
            [DayOfWeek.Sunday] = null,
            [DayOfWeek.Monday] = null,
            [DayOfWeek.Tuesday] = null,
            [DayOfWeek.Wednesday] = null,
            [DayOfWeek.Thursday] = null,
            [DayOfWeek.Friday] = null,
            [DayOfWeek.Saturday] = null,
        };

        do
        {
            var day = (DayOfWeek)reader.GetInt32(6);
            TimeSpan? open = await reader.IsDBNullAsync(7, cancellationToken) ? null : reader.GetTimeSpan(7);
            TimeSpan? close = await reader.IsDBNullAsync(8, cancellationToken) ? null : reader.GetTimeSpan(8);

            if (open.HasValue && close.HasValue)
            {
                schedule[day] = new TimeSlot(open.Value, close.Value);
            }
        }
        while (await reader.ReadAsync(cancellationToken));

        return new Restaurant(
            id,
            name,
            address,
            new WorkSchedule(schedule),
            new DeliveryZone(radius, new Coordinate(lat, lon)));
    }

    public async Task<long> CreateAsync(
        string restaurantName,
        string restaurantAddress,
        DeliveryZone restaurantDeliveryZone,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into restaurants(
                               restaurant_name,
                               restaurant_address,
                               delivery_radius_km,
                               delivery_center_lat,
                               delivery_center_lon)
                           values (
                               :name,
                               :address,
                               :radius_km,
                               :center_lat,
                               :center_lon)
                           returning restaurant_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("name", restaurantName),
                new NpgsqlParameter("address", restaurantAddress),
                new NpgsqlParameter("radius_km", restaurantDeliveryZone.DeliveryRadius),
                new NpgsqlParameter("center_lat", restaurantDeliveryZone.Center.Latitude),
                new NpgsqlParameter("center_lon", restaurantDeliveryZone.Center.Longitude),
            },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? reader.GetInt64(0)
            : throw new RepositoryContractViolationException("RestaurantRepository.CreateAsync");
    }

    public async Task UpdateAsync(
        long restaurantId,
        string? restaurantName,
        string? restaurantAddress,
        DeliveryZone? restaurantDeliveryZone,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           update restaurants
                           set
                               restaurant_name = coalesce(:name, restaurant_name),
                               restaurant_address = coalesce(:address, restaurant_address),
                               delivery_radius_km = coalesce(:radius_km, delivery_radius_km),
                               delivery_center_lat = coalesce(:center_lat, delivery_center_lat),
                               delivery_center_lon = coalesce(:center_lon, delivery_center_lon)
                           where restaurant_id = :restaurant_id;
                           """;

        object nameValue = restaurantName is null ? DBNull.Value : restaurantName;
        object addressValue = restaurantAddress is null ? DBNull.Value : restaurantAddress;
        object radiusValue = restaurantDeliveryZone is null ? DBNull.Value : restaurantDeliveryZone.DeliveryRadius;
        object latValue = restaurantDeliveryZone is null ? DBNull.Value : restaurantDeliveryZone.Center.Latitude;
        object lonValue = restaurantDeliveryZone is null ? DBNull.Value : restaurantDeliveryZone.Center.Longitude;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("restaurant_id", restaurantId),
                new NpgsqlParameter("name", nameValue),
                new NpgsqlParameter("address", addressValue),
                new NpgsqlParameter("radius_km", radiusValue),
                new NpgsqlParameter("center_lat", latValue),
                new NpgsqlParameter("center_lon", lonValue),
            },
        };

        int updated = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updated == 0) throw new RestaurantNotFoundException(restaurantId);
    }

    public async Task DeleteAsync(long restaurantId, CancellationToken cancellationToken)
    {
        const string sql = """
                           delete from restaurants
                           where restaurant_id = :restaurant_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters = { new NpgsqlParameter("restaurant_id", restaurantId) },
        };

        int deleted = await command.ExecuteNonQueryAsync(cancellationToken);
        if (deleted == 0) throw new RestaurantNotFoundException(restaurantId);
    }
}