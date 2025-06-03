using System.Drawing.Imaging;
using ScottPlot;
using System.Globalization;
using LocalWeatherAPI.DataBaseContext;
using LocalWeatherAPI.DTOs;
using LocalWeatherAPI.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;


namespace LocalWeatherAPI.Controllers
{
    [ApiController]
    [Route("/api/data/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> logger;
        private LocalWeatherDataBaseContext db;

        public DataController(ILogger<DataController> logger, LocalWeatherDataBaseContext db)
        {
            this.logger = logger;
            this.db = db;
        }
        [HttpGet("getdatarange")]
        public async Task<IActionResult> GetDatainRange([FromQuery] RangeDTO range)
        {
            string start = range.StartDateTime;
            string end = range.EndDateTime;

            if (!DateTime.TryParse(start, out var startstamp))
            {
                return BadRequest(new
                {
                    success = false,
                    code = Codes.BADREQUEST,
                    msg = "Could not parse start date time range"
                });
            }
            startstamp = startstamp.ToUniversalTime();
            if (!DateTime.TryParse(end, out var endstamp))
            {
                return BadRequest(new
                {
                    success = false,
                    code = Codes.BADREQUEST,
                    msg = "Could not parse end date time range"
                });
            }
            endstamp = endstamp.ToUniversalTime();
            var data = await db.WeatherData.Where(u => u.DateTimeStampID >= startstamp && u.DateTimeStampID <= endstamp).ToListAsync();
            return Ok(new
            {
                success = true,
                code = Codes.OK,
                msg = $"Found data in range {startstamp} to {endstamp}",
                weatherdata = data
            });
        }


        [HttpGet("temp-graph")]
        public async Task<IActionResult> GetTemperatureGraph([FromQuery] RangeDTO range)
        {
            if (!DateTime.TryParse(range.StartDateTime, out var startstamp) || !DateTime.TryParse(range.EndDateTime, out var endstamp))
            {
                return BadRequest(new
                {
                    success = false,
                    code = Codes.BADREQUEST,
                    msg = "Invalid date format"
                });
            }

            startstamp = startstamp.ToUniversalTime();
            endstamp = endstamp.ToUniversalTime();

            var data = await db.WeatherData
                .Where(w => w.DateTimeStampID >= startstamp && w.DateTimeStampID <= endstamp)
                .OrderBy(w => w.DateTimeStampID)
                .ToListAsync();

            if (data.Count == 0)
            {
                return NotFound(new
                {
                    success = false,
                    code = Codes.NOTFOUND,
                    msg = "No data in the specified range"
                });
            }

            // Prepare data for charting
            double[] times = data.Select(d => d.DateTimeStampID.ToOADate()).ToArray();
            double[] temps = data.Select(d => (double)d.Temp).ToArray();

            // Create chart
            var plt = new Plot();
            plt.Add.Scatter(times, temps);
            plt.Title("Temperature Over Time");
            plt.XLabel("Time");
            plt.YLabel("Temperature (°F)");
            plt.Axes.DateTimeTicksBottom();
            plt.Axes.Margins(0.05, 0.2); // Adds slight padding
             // Adds a grid for readability
            


            var assetsDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
            if (!Directory.Exists(assetsDir))
                Directory.CreateDirectory(assetsDir);
            string safeStart = startstamp.ToString("yyyy-MM-dd_HH-mm-ss");
            string safeEnd = endstamp.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"graphrange_{safeStart}_to_{safeEnd}.png";
            var filePath = Path.Combine(assetsDir, fileName);

            plt.SavePng(filePath, 800, 400);

            return PhysicalFile(filePath, "image/png");
        }


    }
}