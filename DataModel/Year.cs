
using System.ComponentModel.DataAnnotations;

namespace LocalWeatherAPI.DataModel
{
    public class Year
    {
        public List<Month>? Months { get; set; } = new();
        [Key]
        public required int YearId { get; set; }

        
    }
}