using FluentMigrator;

namespace RestaurantService.Infrastructure.Persistence.Migrations;

[Migration(1, "Initial RestaurantService schema")]
public class InitialMigration : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            create table restaurants
            (
                restaurant_id bigint primary key generated always as identity,
                restaurant_name text not null,
                restaurant_address text not null,

                delivery_radius_km double precision not null,
                delivery_center_lat double precision not null,
                delivery_center_lon double precision not null
            );

            create table restaurant_working_hours
            (
                working_hours_id bigint primary key generated always as identity,
                restaurant_id bigint  not null references restaurants (restaurant_id),

                day_of_week int not null,

                open_time time null,
                close_time time null
            );

            create type food_category as enum
            (
                'appetizers',
                'main_courses',
                'desserts',
                'beverages',
                'sides'
            );

            create table dishes
            (
                dish_id bigint primary key generated always as identity,
                restaurant_id bigint not null references restaurants (restaurant_id),

                dish_name text not null,
                dish_price bigint not null,
                dish_availability boolean not null,
                food_category food_category not null
            );
            """);
    }

    public override void Down()
    {
        Execute.Sql("""
            drop table if exists dishes;
            drop type if exists food_category;
            drop table if exists restaurant_working_hours;
            drop table if exists restaurants;
            """);
    }
}
