using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LocalWeatherAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YearlyWeatherData",
                columns: table => new
                {
                    YearId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearlyWeatherData", x => x.YearId);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyWeatherData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MonthNumber = table.Column<int>(type: "integer", nullable: false),
                    YearId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyWeatherData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyWeatherData_YearlyWeatherData_YearId",
                        column: x => x.YearId,
                        principalTable: "YearlyWeatherData",
                        principalColumn: "YearId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyWeatherData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MonthId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWeatherData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyWeatherData_MonthlyWeatherData_MonthId",
                        column: x => x.MonthId,
                        principalTable: "MonthlyWeatherData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HourlyWeatherData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HourTime = table.Column<int>(type: "integer", nullable: false),
                    DayId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyWeatherData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HourlyWeatherData_DailyWeatherData_DayId",
                        column: x => x.DayId,
                        principalTable: "DailyWeatherData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeatherData",
                columns: table => new
                {
                    DateTimeStampID = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Temp = table.Column<double>(type: "double precision", nullable: false),
                    Humidity = table.Column<double>(type: "double precision", nullable: false),
                    Pressure = table.Column<double>(type: "double precision", nullable: false),
                    Time = table.Column<int>(type: "integer", nullable: false),
                    HourId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherData", x => x.DateTimeStampID);
                    table.ForeignKey(
                        name: "FK_WeatherData_HourlyWeatherData_HourId",
                        column: x => x.HourId,
                        principalTable: "HourlyWeatherData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyWeatherData_MonthId",
                table: "DailyWeatherData",
                column: "MonthId");

            migrationBuilder.CreateIndex(
                name: "IX_HourlyWeatherData_DayId",
                table: "HourlyWeatherData",
                column: "DayId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyWeatherData_YearId",
                table: "MonthlyWeatherData",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherData_HourId",
                table: "WeatherData",
                column: "HourId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherData");

            migrationBuilder.DropTable(
                name: "HourlyWeatherData");

            migrationBuilder.DropTable(
                name: "DailyWeatherData");

            migrationBuilder.DropTable(
                name: "MonthlyWeatherData");

            migrationBuilder.DropTable(
                name: "YearlyWeatherData");
        }
    }
}
