using LocalWeatherAPI.DataBaseContext;
using LocalWeatherAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LocalWeatherAPI.DBService
{
    public class LocalWeatherDBService
    {
        private LocalWeatherDataBaseContext db;
        private readonly ILogger<LocalWeatherDBService> logger;
        public LocalWeatherDBService(LocalWeatherDataBaseContext db, ILogger<LocalWeatherDBService> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public async Task<WeatherDTO> GetData(DateTime dtStamp)
        {
            var universalStamp = dtStamp.ToUniversalTime();
            var dStamp = DateOnly.FromDateTime(dtStamp);

            var year = await db.YearlyWeatherData
                .Include(y => y.Months!)
                    .ThenInclude(m => m.Days!)
                        .ThenInclude(d => d.DailyWeather!)
                            .ThenInclude(h => h.HourlyWeather)
                .FirstOrDefaultAsync(y => y.YearId == universalStamp.Year);

            if (year is null) year = await db.YearlyWeatherData
                        .Include(y => y.Months!)
                            .ThenInclude(m => m.Days!)
                                .ThenInclude(d => d.DailyWeather!)
                                    .ThenInclude(h => h.HourlyWeather)
                        .LastAsync();

            var month = year.Months!.FirstOrDefault(m => m.MonthNumber == universalStamp.Month);
            if (month is null) month = year.Months!.Last();

            var day = month.Days!.FirstOrDefault(d => d.DayNumber == universalStamp.Day);
            if (day is null) day = month.Days!.Last();

            var hour = day.DailyWeather!.FirstOrDefault(h => h.HourTime == universalStamp.Hour);
            if (hour is null) hour = day.DailyWeather!.Last();

            var weather = hour.HourlyWeather!.LastOrDefault(w =>
                w.DateTimeStampID.Date == universalStamp.Date &&
                w.DateTimeStampID.Hour == universalStamp.Hour);

            if (weather is null) weather = hour.HourlyWeather!.Last();

            WeatherDTO dto = new WeatherDTO
            {
                Temp = weather.Temp,
                Humidity = weather.Humidity,
                Pressure = weather.Pressure,
                Stamp = weather.DateTimeStampID.ToString()
            };

            return dto;
        }

        public async Task<List<WeatherDTO>> GetDataRange(DateTime start, DateTime end)
        {
            var data = await db.WeatherData.Where(u => u.DateTimeStampID >= start && u.DateTimeStampID <= end).ToListAsync();
            List<WeatherDTO> datalist = new List<WeatherDTO>();
            foreach (var d in data)
            {
                datalist.Add(new WeatherDTO
                {
                    Temp = d.Temp,
                    Humidity = d.Humidity,
                    Pressure = d.Pressure,
                    Stamp = d.DateTimeStampID.ToString()
                });
            }
            return datalist;
        }
    }
}