using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NarushiteliZavoda.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public double? HoursWorked { get; set; }

        public int EmployeeId { get; set; }

        [JsonIgnore]
        public Employee? Employee { get; set; }
    }
}