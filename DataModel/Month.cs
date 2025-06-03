

using System.ComponentModel.DataAnnotations;

namespace LocalWeatherAPI.DataModel
{
    public class Month
    {
        [Key]
        public int Id { get; set; }
        public required int MonthNumber { get; set; }

        public int YearId { get; set; }
        public Year? Year { get; set; }

        public List<Day>? Days { get; set; } = new();
    }
}