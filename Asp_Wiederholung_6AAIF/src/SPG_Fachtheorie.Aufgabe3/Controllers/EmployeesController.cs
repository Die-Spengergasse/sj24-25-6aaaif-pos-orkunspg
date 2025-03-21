using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe3.Dtos;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]  // [controller] --> Name vor "Controller" --> /api/employees
    [ApiController]              // Wird als controller berücksichtigt.
    public class EmployeesController : ControllerBase
    {
        private readonly AppointmentContext _db;
        public EmployeesController(AppointmentContext db)
        {
            _db = db;
        }

        // Verknüpfen von Adresse mit Methode.
        // HttpGet -> GET Request
        // Keine Adresse -> Adresse des Controllers
        // ==> Reagiert auf GET /api/employees
        //                  GET /api/employees?type=manager
        //                  GET /api/employees?type=MANAGER
        [HttpGet]
        public ActionResult<List<EmployeeDto>> GetAllEmployees([FromQuery] string? type)
        {
            return Ok(_db.Employees
                .Where(e => string.IsNullOrEmpty(type) ? true : e.Type.ToLower() == type.ToLower())
                .Select(e => new EmployeeDto(
                    e.RegistrationNumber, e.Type,
                    e.LastName,
                    e.FirstName))
                .ToList());
        }

        /// <summary>
        /// /api/employees/1001 --> 1001 wird in registrationNumber gelegt.
        /// </summary>
        /// <param name="registrationNumber"></param>
        /// <returns></returns>
        [HttpGet("{registrationNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<EmployeeDetailDto> GetEmployeeById(int registrationNumber)
        {
            var employee = _db.Employees
                .Where(e => e.RegistrationNumber == registrationNumber)
                .Select(e => new EmployeeDetailDto(
                    e.RegistrationNumber, e.LastName,
                    e.FirstName, e.Address,
                    e.Payments.Count()))
                .AsNoTracking()
                .FirstOrDefault();
            if (employee is null)
                return NotFound();  // Return 404 not found.
            return Ok(employee);
        }

    }
}
