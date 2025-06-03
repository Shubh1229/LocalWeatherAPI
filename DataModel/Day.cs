

using System.ComponentModel.DataAnnotations;

namespace LocalWeatherAPI.DataModel
{
    public class Day
    {
        [Key]
        public int Id { get; set; }

        public required int DayNumber { get; set; }

        public DateOnly Date { get; set; }

        public int MonthId { get; set; }
        public Month? Month { get; set; }

        public List<Hour>? DailyWeather { get; set; } = new();
    }

}