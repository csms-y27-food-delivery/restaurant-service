using Npgsql;
using RestaurantService.Application.Abstractions.Repositories;
using RestaurantService.Application.Models.Restaurants;

namespace RestaurantService.Infrastructure.Persistence.Repositories;

public sealed class RestaurantScheduleRepository : IRestaurantScheduleRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public RestaurantScheduleRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task ReplaceAsync(long restaurantId, WorkSchedule schedule, CancellationToken cancellationToken)
    {
        const string sql = """
                           with deleted as (
                               delete from restaurant_working_hours
                               where restaurant_id = :restaurant_id
                           )
                           insert into restaurant_working_hours(
                               restaurant_id,
                               day_of_week,
                               open_time,
                               close_time)
                           select
                               :restaurant_id,
                               x.ord - 1 as day_of_week,
                               x.open_time,
                               x.close_time
                           from unnest(:open_time, :close_time) with ordinality
                                as x(open_time, close_time, ord);
                           """;

        var openTime = new TimeSpan?[7];
        var closeTime = new TimeSpan?[7];

        foreach ((DayOfWeek day, TimeSlot? slot) in schedule.DailySchedules)
        {
            int i = (int)day;
            openTime[i] = slot?.OpenTime;
            closeTime[i] = slot?.CloseTime;
        }

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("restaurant_id", restaurantId),
                new NpgsqlParameter("open_time", openTime),
                new NpgsqlParameter("close_time", closeTime),
            },
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteByRestaurantIdAsync(long restaurantId, CancellationToken cancellationToken)
    {
        const string sql = """
                           delete from restaurant_working_hours
                           where restaurant_id = :restaurant_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters = { new NpgsqlParameter("restaurant_id", restaurantId) },
        };

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}