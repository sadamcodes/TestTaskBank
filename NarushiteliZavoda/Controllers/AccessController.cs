using Microsoft.AspNetCore.Mvc;
using NarushiteliZavoda.Data;
using NarushiteliZavoda.Models;

namespace NarushiteliZavoda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccessController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("start")]
        public IActionResult StartShift([FromBody] int employeeId)
        {
            //Проверка на существование сотрудника
            var employee = _context.Employees.Find(employeeId);
            if (employee == null)
                return BadRequest("Сотрудник не найден");

            var openShift = _context.Shifts.FirstOrDefault(s => s.EmployeeId == employeeId && s.EndTime == null);
            if (openShift != null)
                return BadRequest("У сотрудника уже есть открытая смена");

            var shift = new Shift
            {
                EmployeeId = employeeId,
                StartTime = DateTime.UtcNow
            };

            _context.Shifts.Add(shift);
            _context.SaveChanges();

            return Ok("Смена начата");
        }

        [HttpPost("end")]
        public IActionResult EndShift([FromBody] int employeeId)
        {
            var employee = _context.Employees.Find(employeeId);
            if (employee == null)
                return BadRequest("Сотрудник не найден");

            var shift = _context.Shifts.FirstOrDefault(s => s.EmployeeId == employeeId && s.EndTime == null);
            if (shift == null)
                return BadRequest("Нет открытой смены");

            shift.EndTime = DateTime.UtcNow;

            if (shift.StartTime != null && shift.EndTime != null)
            {
                shift.HoursWorked = (shift.EndTime - shift.StartTime).Value.TotalHours;
            }

            _context.SaveChanges();

            return Ok("Смена закрыта");
        }
    }
}