using Microsoft.EntityFrameworkCore;
using LocalWeatherAPI.DataModel;

namespace LocalWeatherAPI.DataBaseContext
{
    public class LocalWeatherDataBaseContext : DbContext
    {
        public LocalWeatherDataBaseContext(DbContextOptions<LocalWeatherDataBaseContext> options) : base(options)
        {

        }

        public DbSet<Weather> WeatherData { get; set; }
        public DbSet<Hour> HourlyWeatherData { get; set; }
        public DbSet<Day> DailyWeatherData { get; set; }
        public DbSet<Month> MonthlyWeatherData { get; set; }
        public DbSet<Year> YearlyWeatherData { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Day>()
                .HasMany(d => d.DailyWeather)
                .WithOne(h => h.Day)
                .HasForeignKey(h => h.DayId);

            modelBuilder.Entity<Hour>()
                .HasMany(h => h.HourlyWeather)
                .WithOne(w => w.Hour)
                .HasForeignKey(w => w.HourId);

            modelBuilder.Entity<Month>()
                .HasMany(m => m.Days)
                .WithOne(d => d.Month)
                .HasForeignKey(d => d.MonthId);

            modelBuilder.Entity<Year>()
                .HasMany(y => y.Months)
                .WithOne(m => m.Year)
                .HasForeignKey(m => m.YearId);
        }

    }
}