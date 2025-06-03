namespace LocalWeatherAPI.DTOs
{
    public class WeatherDTO
    {
        public required double Temp { get; set; }
        public required double Humidity { get; set; }
        public required double Pressure { get; set; }
        public required string Stamp { get; set; }
    }
}