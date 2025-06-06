
using System.Net;
using LocalWeatherAPI.DataBaseContext;
using LocalWeatherAPI.DataModel;
using LocalWeatherAPI.DTOs;
using LocalWeatherAPI.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using Npgsql.Internal;

namespace LocalWeatherAPI.Controllers
{
    [ApiController]
    [Route("api/esp32bme280client/[controller]")]
    public class Esp32Bme280HttpClientController : ControllerBase
    {
        private readonly ILogger<Esp32Bme280HttpClientController> logger;
        private LocalWeatherDataBaseContext db;

        public Esp32Bme280HttpClientController(ILogger<Esp32Bme280HttpClientController> logger, LocalWeatherDataBaseContext db)
        {
            this.logger = logger;
            this.db = db;
        }
        [HttpGet("currentweather")]
        public async Task<IActionResult> GetWeather()
        {
            var currentDT = DateTime.UtcNow;
            var currentDate = DateOnly.FromDateTime(currentDT);

            var year = await db.YearlyWeatherData
                .Include(y => y.Months!)
                    .ThenInclude(m => m.Days!)
                        .ThenInclude(d => d.DailyWeather!)
                            .ThenInclude(h => h.HourlyWeather)
                .FirstOrDefaultAsync(y => y.YearId == currentDT.Year);

            if (year is null) return NotFound("Could not find year");

            var month = year.Months!.FirstOrDefault(m => m.MonthNumber == currentDT.Month);
            if (month is null) return NotFound("Could not find month");

            var day = month.Days!.FirstOrDefault(d => d.DayNumber == currentDT.Day);
            if (day is null) return NotFound("Could not find day");

            var hour = day.DailyWeather!.FirstOrDefault(h => h.HourTime == currentDT.Hour);
            if (hour is null) return NotFound("Could not find hour");

            var weather = hour.HourlyWeather!.LastOrDefault(w =>
                w.DateTimeStampID.Date == currentDT.Date &&
                w.DateTimeStampID.Hour == currentDT.Hour);

            if (weather is null) return NotFound("Could not find weather");

            WeatherDTO dto = new WeatherDTO
            {
                Temp = weather.Temp,
                Humidity = weather.Humidity,
                Pressure = weather.Pressure,
                Stamp = weather.DateTimeStampID.ToString()
            };

            return Ok(new
            {
                success = true,
                weather = dto
            });
        }

        [HttpPost("addweatherdata/temp={temp}/humidity={humidity}/pressure={pressure}")]
        public async Task<IActionResult> SendWeatherData(double temp, double humidity, double pressure)
        {
            var currentDT = DateTime.UtcNow;

            // Get or create Year
            var year = await db.YearlyWeatherData
                .Include(y => y.Months)
                .FirstOrDefaultAsync(y => y.YearId == currentDT.Year);

            if (year == null)
            {
                year = new Year { YearId = currentDT.Year, Months = new List<Month>() };
                db.YearlyWeatherData.Add(year);
                await db.SaveChangesAsync();
            }

            // Get or create Month
            var month = year.Months.FirstOrDefault(m => m.MonthNumber == currentDT.Month);
            if (month == null)
            {
                month = new Month
                {
                    MonthNumber = currentDT.Month,
                    YearId = year.YearId,
                    Year = year,
                    Days = new List<Day>()
                };
                db.MonthlyWeatherData.Add(month);
                await db.SaveChangesAsync();
            }

            // Get or create Day
            var day = await db.DailyWeatherData
                .Include(d => d.DailyWeather)
                .FirstOrDefaultAsync(d => d.DayNumber == currentDT.Day && d.MonthId == month.Id);

            if (day == null)
            {
                day = new Day
                {
                    DayNumber = currentDT.Day,
                    Date = DateOnly.FromDateTime(currentDT),
                    MonthId = month.Id,
                    Month = month,
                    DailyWeather = new List<Hour>()
                };
                db.DailyWeatherData.Add(day);
                await db.SaveChangesAsync();
            }

            // Get or create Hour
            var hour = day.DailyWeather.FirstOrDefault(h => h.HourTime == currentDT.Hour);
            if (hour == null)
            {
                hour = new Hour
                {
                    HourTime = currentDT.Hour,
                    DayId = day.Id,
                    Day = day,
                    HourlyWeather = new List<Weather>()
                };
                db.HourlyWeatherData.Add(hour);
                await db.SaveChangesAsync();
            }

            // Create Weather entry
            var weather = new Weather
            {
                Temp = temp,
                Humidity = humidity,
                Pressure = pressure,
                DateTimeStampID = currentDT,
                Time = currentDT.Minute,
                HourId = hour.Id,
                Hour = hour
            };

            db.WeatherData.Add(weather);
            await db.SaveChangesAsync();

            WeatherDTO dto = new WeatherDTO
            {
                Temp = weather.Temp,
                Humidity = weather.Humidity,
                Pressure = weather.Pressure,
                Stamp = weather.DateTimeStampID.ToString()
            };

            return Ok(new
            {
                success = true,
                weather = dto
            });
        }

        [HttpPost("sensordata")]
        public async Task<IActionResult> PersistSensorData([FromBody] WeatherDTO dto)
        {
            if (dto == null)
            {
                logger.LogInformation($"Invalid data from sensor data was null...");
                return Ok(new
                {
                    success = false,
                    code = Codes.BADREQUEST,
                    msg = "dto was null"
                });
            }
            if (dto.Temp <= -100 || dto.Temp >= 110 || dto.Humidity < 0 || dto.Humidity > 100 || dto.Pressure < 800 || dto.Pressure > 1200)
            {
                logger.LogInformation($"Data was out of range: Temp {dto.Temp}, Humidity: {dto.Humidity}, Pressure: {dto.Pressure}, Stamp: {dto.Stamp}");
                return BadRequest(new
                {
                    success = false,
                    code = Codes.BADREQUEST,
                    msg = "Invalid sensor data"
                });
            }
            if (!DateTime.TryParse(dto.Stamp, out var dateTime))
            {
                logger.LogInformation($"Could not parse date time stamp: {dto.Stamp}");
                return BadRequest(new
                {
                    success = false,
                    code = Codes.BADREQUEST,
                    msg = "Invalid timestamp format"
                });
            }
            dateTime = dateTime.ToUniversalTime();
            logger.LogInformation($"Properly Recieved {dto} and converted stamp {dateTime}");
            // Get or create Year
            var year = await db.YearlyWeatherData
                .Include(y => y.Months)
                .FirstOrDefaultAsync(y => y.YearId == dateTime.Year);

            if (year == null)
            {
                year = new Year { YearId = dateTime.Year, Months = new List<Month>() };
                db.YearlyWeatherData.Add(year);
                await db.SaveChangesAsync();
            }

            // Get or create Month
            var month = year.Months.FirstOrDefault(m => m.MonthNumber == dateTime.Month);
            if (month == null)
            {
                month = new Month
                {
                    MonthNumber = dateTime.Month,
                    YearId = year.YearId,
                    Year = year,
                    Days = new List<Day>()
                };
                db.MonthlyWeatherData.Add(month);
                await db.SaveChangesAsync();
            }

            // Get or create Day
            var day = await db.DailyWeatherData
                .Include(d => d.DailyWeather)
                .FirstOrDefaultAsync(d => d.DayNumber == dateTime.Day && d.MonthId == month.Id);

            if (day == null)
            {
                day = new Day
                {
                    DayNumber = dateTime.Day,
                    Date = DateOnly.FromDateTime(dateTime),
                    MonthId = month.Id,
                    Month = month,
                    DailyWeather = new List<Hour>()
                };
                db.DailyWeatherData.Add(day);
                await db.SaveChangesAsync();
            }

            // Get or create Hour
            var hour = day.DailyWeather.FirstOrDefault(h => h.HourTime == dateTime.Hour);
            if (hour == null)
            {
                hour = new Hour
                {
                    HourTime = dateTime.Hour,
                    DayId = day.Id,
                    Day = day,
                    HourlyWeather = new List<Weather>()
                };
                db.HourlyWeatherData.Add(hour);
                await db.SaveChangesAsync();
            }

            // Create Weather entry
            var weather = new Weather
            {
                Temp = dto.Temp,
                Humidity = dto.Humidity,
                Pressure = dto.Pressure,
                DateTimeStampID = dateTime,
                Time = dateTime.Minute,
                HourId = hour.Id,
                Hour = hour
            };

            db.WeatherData.Add(weather);
            await db.SaveChangesAsync();
            logger.LogInformation($"Successfully added {weather} to database!");
            return Ok(new
            {
                success = true,
                code = Codes.OK,
                msg = "Nice Job!"
            });
        }

        private bool IsNull(object o)
        {
            if (o == null)
            {
                return true;
            }
            return false;
        }

    }
}