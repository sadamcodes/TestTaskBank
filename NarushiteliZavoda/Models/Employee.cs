using System.ComponentModel.DataAnnotations;
namespace NarushiteliZavoda.Models
{
    public enum Position
    {
        Maneger,
        Engineer,
        CandleTester

    }
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public string? MiddleName { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;



        [Required]
        public Position Position { get; set; }

        public List<Shift> Shifts { get; set; } = new();
    }
}