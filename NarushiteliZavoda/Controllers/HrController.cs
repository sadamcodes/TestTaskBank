using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NarushiteliZavoda.Data;
using NarushiteliZavoda.Models;

namespace NarushiteliZavoda.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HrController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HrController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public IActionResult AddEmployee([FromBody] Employee employee)
        {
            if (string.IsNullOrEmpty(employee.LastName) || string.IsNullOrEmpty(employee.FirstName))
                return BadRequest("Фамилия и Имя обязательны");

            if (!Enum.IsDefined(typeof(Position), employee.Position))
                return BadRequest("Некорректная должность");

            _context.Employees.Add(employee);
            _context.SaveChanges();

            return Ok(employee);
        }

        [HttpPut("update")]
        public IActionResult UpdateEmployee([FromBody] Employee updated)
        {
            var employee = _context.Employees.Find(updated.Id);

            if (employee == null)
                return BadRequest("Сотрудник не найден");

            employee.LastName = updated.LastName;
            employee.FirstName = updated.FirstName;
            employee.MiddleName = updated.MiddleName;
            employee.Position = updated.Position;

            _context.SaveChanges();

            return Ok(employee);
        }

        [HttpDelete("delete/{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee == null)
                return BadRequest("Сотрудник не найден");

            _context.Employees.Remove(employee);
            _context.SaveChanges();

            return Ok("Сотрудник удалён");
        }

        [HttpGet("list")]
        public IActionResult GetEmployees([FromQuery] string? position = null)
        {
            var query = _context.Employees
                .Include(e => e.Shifts)
                .AsQueryable();

            if (!string.IsNullOrEmpty(position))
            {
                if (!Enum.TryParse<Position>(position, true, out var pos))
                    return BadRequest("Такой должности нет");

                query = query.Where(e => e.Position == pos);
            }

            var employees = query.ToList();

            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            var result = new List<object>();

            foreach (var emp in employees)
            {
                int violations = 0;

                var recentShifts = emp.Shifts
                    .Where(s => s.StartTime >= oneMonthAgo && s.EndTime.HasValue);

                foreach (var shift in recentShifts)
                {
                    bool hasViolation = false;

                    if (emp.Position == Position.CandleTester)
                    {
                        if (shift.StartTime.TimeOfDay > new TimeSpan(9, 0, 0) ||
                            (shift.EndTime.HasValue &&
                             shift.EndTime.Value.TimeOfDay < new TimeSpan(21, 0, 0)))
                        {
                            hasViolation = true;
                        }
                    }
                    else
                    {
                        if (shift.StartTime.TimeOfDay > new TimeSpan(9, 0, 0) ||
                            (shift.EndTime.HasValue &&
                             shift.EndTime.Value.TimeOfDay < new TimeSpan(18, 0, 0)))
                        {
                            hasViolation = true;
                        }
                    }

                    if (hasViolation)
                        violations++;
                }
                result.Add(new
                {
                    emp.Id,
                    emp.LastName,
                    emp.FirstName,
                    emp.MiddleName,
                    Position = emp.Position.ToString(),
                    ShiftCount = emp.Shifts.Count,
                    ViolationsLastMonth = violations,
                    Shifts = emp.Shifts
                });
            }

            return Ok(result);
        }

        [HttpGet("positions")]
        public IActionResult GetPositions()
        {
            var positions = Enum.GetValues(typeof(Position))
                .Cast<Position>()
                .Select(p => p.ToString())
                .ToList();

            return Ok(positions);
        }
    }
}