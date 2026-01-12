using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RestaurantService.Application.Abstractions.Repositories;
using RestaurantService.Application.Models.Menu;
using RestaurantService.Application.Models.Restaurants;
using RestaurantService.Application.Models.Results;
using RestaurantService.Application.RestaurantServices;
using RestaurantService.Infrastructure.Persistence.Exceptions;
using Xunit;

namespace RestaurantService.Tests;

public sealed class RestaurantValidateServiceTests
{
    // Проверяет: если в заказе нет блюд (пустой список), сервис должен вернуть Fail с сообщением "Order has no dishes."
    [Fact]
    public async Task Empty_Dishes_Should_Fail()
    {
        // arrange
        IRestaurantRepository restaurants = Substitute.For<IRestaurantRepository>();
        IDishRepository dishes = Substitute.For<IDishRepository>();
        TimeProvider time = Substitute.For<TimeProvider>();

        var svc = new RestaurantValidateService(restaurants, dishes, time);

        // act
        OrderValidationResult result = await svc.ValidateOrderAsync(
            restaurantId: 1,
            dishNames: [],
            customerLocation: new Coordinate(0, 0),
            cancellationToken: CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Description.Should().Be("Order has no dishes.");
        result.Dishes.Should().BeEmpty();
    }

    // Проверяет: если ресторан не найден (репозиторий вернул null), сервис должен вернуть Fail с сообщением "Restaurant does not exist."
    [Fact]
    public async Task Restaurant_Not_Found_Should_Throw()
    {
        // arrange
        IRestaurantRepository restaurants = Substitute.For<IRestaurantRepository>();
        IDishRepository dishes = Substitute.For<IDishRepository>();
        TimeProvider time = Substitute.For<TimeProvider>();

        restaurants.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RestaurantNotFoundException(1));

        var svc = new RestaurantValidateService(restaurants, dishes, time);

        // act
        Func<Task> act = async () => await svc.ValidateOrderAsync(
            restaurantId: 1,
            dishNames: ["pizza"],
            customerLocation: new Coordinate(0, 0),
            cancellationToken: CancellationToken.None);

        // assert
        await act.Should()
            .ThrowAsync<RestaurantNotFoundException>()
            .WithMessage("Restaurant with id='1' was not found.");
    }

    // Проверяет: если ресторан найден, но по расписанию он закрыт, сервис должен вернуть Fail "Restaurant is closed."
    [Fact]
    public async Task Restaurant_Closed_Should_Fail()
    {
        // arrange
        IRestaurantRepository restaurants = Substitute.For<IRestaurantRepository>();
        IDishRepository dishes = Substitute.For<IDishRepository>();

        var now = new DateTimeOffset(2026, 01, 03, 12, 00, 00, TimeSpan.Zero);
        TimeProvider time = new TestTimeProvider(now);

        WorkSchedule schedule = MakeSchedule(now.DayOfWeek, TimeSpan.FromHours(8), TimeSpan.FromHours(9));
        var zone = new DeliveryZone(1000, new Coordinate(0, 0));

        var restaurant = new Restaurant(1, "BebraRest", "Bebra 12", schedule, zone);
        restaurants.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(restaurant);

        var svc = new RestaurantValidateService(restaurants, dishes, time);

        // act
        OrderValidationResult result = await svc.ValidateOrderAsync(
            restaurantId: 1,
            dishNames: ["pizza"],
            customerLocation: new Coordinate(0, 0),
            cancellationToken: CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Description.Should().Be("Restaurant is closed.");
        result.Dishes.Should().BeEmpty();
    }

    private static readonly string[] DishNames = new[] { "pizza" };
    private static readonly string[] DishNamesArray = new[] { "pizza", "soup" };
    private static readonly string[] DishNamesArray0 = new[] { " Pizza ", "soup", "pizza" };

    // Проверяет: если ресторан открыт, но доставка по координатам недоступна, сервис должен вернуть Fail "Delivery is not available for this location."
    [Fact]
    public async Task Delivery_Not_Available_Should_Fail()
    {
        // arrange
        IRestaurantRepository restaurants = Substitute.For<IRestaurantRepository>();
        IDishRepository dishes = Substitute.For<IDishRepository>();

        var now = new DateTimeOffset(2026, 01, 03, 12, 00, 00, TimeSpan.Zero);
        TimeProvider time = new TestTimeProvider(now);

        WorkSchedule schedule = MakeSchedule(
            now.DayOfWeek,
            TimeSpan.Zero,
            TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59));

        var zone = new DeliveryZone(0, new Coordinate(0, 0));

        var restaurant = new Restaurant(1, "PepaShop", "FaVatafa 13", schedule, zone);
        restaurants.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(restaurant);

        var svc = new RestaurantValidateService(restaurants, dishes, time);

        // act
        OrderValidationResult result = await svc.ValidateOrderAsync(
            restaurantId: 1,
            dishNames: DishNames,
            customerLocation: new Coordinate(1, 1),
            cancellationToken: CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Description.Should().Be("Delivery is not available for this location.");
        result.Dishes.Should().BeEmpty();
    }

    // Проверяет: если ресторан открыт и доставка доступна, но часть блюд не найдена, сервис должен вернуть Fail и перечислить missing блюда в Description.
    [Fact]
    public async Task Missing_Dishes_Should_Fail_And_List_Missing()
    {
        // arrange
        IRestaurantRepository restaurants = Substitute.For<IRestaurantRepository>();
        IDishRepository dishes = Substitute.For<IDishRepository>();

        var now = new DateTimeOffset(2026, 01, 03, 12, 00, 00, TimeSpan.Zero);
        TimeProvider time = new TestTimeProvider(now);

        WorkSchedule schedule = MakeSchedule(
            now.DayOfWeek,
            TimeSpan.Zero,
            TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59));

        var zone = new DeliveryZone(9999, new Coordinate(0, 0));
        var restaurant = new Restaurant(1, "Papa", "moshet 21", schedule, zone);

        restaurants.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(restaurant);

        string[] requested = [" Pizza ", "Pepa Shnele"];

        dishes.GetByNamesAsync(1, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Dish>
            {
                new Dish(10, "pizza", 100, true, 1, FoodCategory.MainCourses),
            });

        var svc = new RestaurantValidateService(restaurants, dishes, time);

        // act
        OrderValidationResult result = await svc.ValidateOrderAsync(1, requested, new Coordinate(0, 0), CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Description.Should().StartWith("Some dishes do not exist for this restaurant:");
        result.Description.Should().Contain("pepa shnele");
        result.Dishes.Should().BeEmpty();
    }

    // Проверяет: если все блюда найдены, но среди них есть недоступные (DishAvailability=false),
    // сервис должен вернуть Fail "Some dishes are not available: ..." и НЕ возвращать список блюд.
    [Fact]
    public async Task Unavailable_Dishes_Should_Fail()
    {
        // arrange
        IRestaurantRepository restaurants = Substitute.For<IRestaurantRepository>();
        IDishRepository dishes = Substitute.For<IDishRepository>();

        var now = new DateTimeOffset(2026, 01, 03, 12, 00, 00, TimeSpan.Zero);
        TimeProvider time = new TestTimeProvider(now);

        WorkSchedule schedule = MakeSchedule(
            now.DayOfWeek,
            TimeSpan.Zero,
            TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59));

        var zone = new DeliveryZone(9999, new Coordinate(0, 0));
        var restaurant = new Restaurant(1, "DoIt", "Aga 12", schedule, zone);

        restaurants.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(restaurant);

        dishes.GetByNamesAsync(1, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Dish>
            {
                new Dish(10, "pizza", 100, false, 1, FoodCategory.MainCourses),
                new Dish(11, "soup", 50, true, 1, FoodCategory.Appetizers),
            });

        var svc = new RestaurantValidateService(restaurants, dishes, time);

        // act
        OrderValidationResult result = await svc.ValidateOrderAsync(
            1,
            DishNamesArray,
            new Coordinate(0, 0),
            CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeFalse();
        result.Description.Should().Be("Some dishes are not available: pizza");
        result.Dishes.Should().BeEmpty();
    }

    // Проверяет: happy-path — ресторан существует, открыт, доставка доступна, все блюда существуют и доступны.
    // Сервис должен вернуть Success и список блюд в ИСХОДНОМ порядке запроса, включая дубликаты.
    [Fact]
    public async Task Happy_Path_Should_Return_Dishes_In_Request_Order_Including_Duplicates()
    {
        // arrange
        IRestaurantRepository restaurants = Substitute.For<IRestaurantRepository>();
        IDishRepository dishes = Substitute.For<IDishRepository>();

        var now = new DateTimeOffset(2026, 01, 03, 12, 00, 00, TimeSpan.Zero);
        TimeProvider time = new TestTimeProvider(now);

        WorkSchedule schedule = MakeSchedule(
            now.DayOfWeek,
            TimeSpan.Zero,
            TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59));

        var zone = new DeliveryZone(9999, new Coordinate(0, 0));
        var restaurant = new Restaurant(1, "HappyHouse", "R", schedule, zone);

        restaurants.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(restaurant);

        var pizza = new Dish(10, "pizza", 100, true, 1, FoodCategory.MainCourses);
        var soup = new Dish(11, "soup", 50, true, 1, FoodCategory.Appetizers);

        dishes.GetByNamesAsync(1, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Dish> { pizza, soup });

        var svc = new RestaurantValidateService(restaurants, dishes, time);

        // act
        OrderValidationResult result = await svc.ValidateOrderAsync(
            1,
            DishNamesArray0,
            new Coordinate(0, 0),
            CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Description.Should().BeNull();
        result.Dishes.Select(dish => dish.DishId).Should().Equal(10, 11, 10);
    }

    private static WorkSchedule MakeSchedule(DayOfWeek day, TimeSpan open, TimeSpan close)
    {
        return new WorkSchedule(new Dictionary<DayOfWeek, TimeSlot?> { [day] = new TimeSlot(open, close) });
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        public TestTimeProvider(DateTimeOffset now, TimeZoneInfo? localTimeZone = null)
        {
            LocalTimeZone = localTimeZone ?? TimeZoneInfo.Utc;

            UtcNow = now.ToUniversalTime();
        }

        public override TimeZoneInfo LocalTimeZone { get; }

        public override DateTimeOffset GetUtcNow()
        {
            return UtcNow;
        }

        private DateTimeOffset UtcNow { get; }
    }
}