using Npgsql;
using RestaurantService.Application.Abstractions.Repositories;
using RestaurantService.Application.Models.Menu;
using RestaurantService.Infrastructure.Persistence.Exceptions;

namespace RestaurantService.Infrastructure.Persistence.Repositories;

public sealed class DishRepository : IDishRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public DishRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Dish> GetByIdAsync(long dishId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select dish_id, dish_name, dish_price, dish_availability, restaurant_id, food_category
                           from dishes
                           where dish_id = :dish_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters = { new NpgsqlParameter("dish_id", dishId) },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return !await reader.ReadAsync(cancellationToken)
            ? throw new DishNotFoundException(dishId)
            : new Dish(
            reader.GetInt64(0),
            reader.GetString(1),
            reader.GetInt64(2),
            reader.GetBoolean(3),
            reader.GetInt64(4),
            await reader.GetFieldValueAsync<FoodCategory>(5, cancellationToken));
    }

    public async Task<IReadOnlyList<Dish>> GetByNamesAsync(
        long restaurantId,
        IReadOnlyCollection<string> dishNames,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           select dish_id, dish_name, dish_price, dish_availability, restaurant_id, food_category
                           from dishes
                           where restaurant_id = :restaurant_id and lower(dish_name) = any(:dish_names);
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("restaurant_id", restaurantId),
                new NpgsqlParameter("dish_names", dishNames),
            },
        };

        var result = new List<Dish>(dishNames.Count);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new Dish(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetInt64(2),
                reader.GetBoolean(3),
                reader.GetInt64(4),
                await reader.GetFieldValueAsync<FoodCategory>(5, cancellationToken)));
        }

        return result;
    }

    public async Task<long> CreateAsync(
        long restaurantId,
        string dishName,
        long dishPrice,
        bool dishAvailability,
        FoodCategory foodCategory,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into dishes(restaurant_id, dish_name, dish_price, dish_availability, food_category)
                           values (:restaurant_id, :dish_name, :dish_price, :dish_availability, :food_category)
                           returning dish_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("restaurant_id", restaurantId),
                new NpgsqlParameter("dish_name", dishName),
                new NpgsqlParameter("dish_price", dishPrice),
                new NpgsqlParameter("dish_availability", dishAvailability),
                new NpgsqlParameter("food_category", foodCategory),
            },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? reader.GetInt64(0)
            : throw new RepositoryContractViolationException("DishRepository.CreateAsync");
    }

    public async Task UpdateAsync(
        long dishId,
        long? dishPrice,
        bool? dishAvailability,
        FoodCategory? foodCategory,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           update dishes
                           set
                               dish_price = coalesce(:dish_price, dish_price),
                               dish_availability = coalesce(:dish_availability, dish_availability),
                               food_category = coalesce(:food_category, food_category)
                           where dish_id = :dish_id;
                           """;

        object priceValue = dishPrice is null ? DBNull.Value : dishPrice;
        object availabilityValue = dishAvailability is null ? DBNull.Value : dishAvailability;
        object foodCategoryValue = foodCategory is null ? DBNull.Value : foodCategory;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("dish_id", dishId),
                new NpgsqlParameter("dish_price", priceValue),
                new NpgsqlParameter("dish_availability", availabilityValue),
                new NpgsqlParameter("food_category", foodCategoryValue),
            },
        };

        int updated = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updated == 0) throw new DishNotFoundException(dishId);
    }

    public async Task DeleteAsync(long dishId, CancellationToken cancellationToken)
    {
        const string sql = """
                           delete from dishes
                           where dish_id = :dish_id;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters = { new NpgsqlParameter("dish_id", dishId) },
        };

        int deleted = await command.ExecuteNonQueryAsync(cancellationToken);
        if (deleted == 0) throw new DishNotFoundException(dishId);
    }
}