

using System.ComponentModel.DataAnnotations;

namespace LocalWeatherAPI.DataModel
{
    public class Hour
    {
        [Key]
        public int Id { get; set; }

        public required int HourTime { get; set; }

        public int DayId { get; set; }
        public Day? Day { get; set; }

        public List<Weather>? HourlyWeather { get; set; } = new();
    }
}