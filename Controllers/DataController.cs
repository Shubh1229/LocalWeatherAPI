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
        private string STARTDATE = "2025-05-21T03:59:00Z";

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


        [HttpGet("alldata")]
        public async Task<IActionResult> GetAllData()
        {
            DateTime dt = DateTime.Parse(STARTDATE, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            var data = await db.WeatherData.Where(u => u.DateTimeStampID >= dt).ToListAsync();
            if (data.Count == 0)
            {
                return BadRequest("Could not find data");
            }
            return Ok(new
            {
                success = true,
                code = Codes.OK,
                msg = $"Found data in range {data[0].DateTimeStampID} to {data.Last().DateTimeStampID}",
                weatherdata = data
            });
        }


    }
}