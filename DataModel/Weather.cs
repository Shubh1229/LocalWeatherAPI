using System.ComponentModel.DataAnnotations;

namespace LocalWeatherAPI.DataModel
{
    public class Weather
    {
        public required double Temp { get; set; }
        public required double Humidity { get; set; }
        public required double Pressure { get; set; }
        [Key]
        public required DateTime DateTimeStampID { get; set; }
        public required int Time { get; set; }

        public int HourId { get; set; }
        public Hour? Hour { get; set; }
    }
}