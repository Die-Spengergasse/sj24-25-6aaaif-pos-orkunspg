using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;       // Für Include() und FirstOrDefaultAsync()
using SPG_Fachtheorie.Aufgabe1.Infrastructure; // dein DbContext (AppointmentContext)
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using System.Net; // optional, wenn du HttpStatusCode nutzt

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppointmentContext _db;

        public PaymentsController(AppointmentContext db)
        {
            _db = db;
        }

        // BEREITS VORHANDEN:
        [HttpGet]
        public ActionResult<IEnumerable<PaymentDto>> GetPayments([FromQuery] int? number, [FromQuery] DateTime? dateFrom)
        {
            var result = _db.Payments
                .Where(p => number == null || p.CashDesk.Number == number)
                .Where(p => dateFrom == null || p.PaymentDateTime >= dateFrom)
                .Select(p => new PaymentDto(
                    p.Id,
                    p.Employee.FirstName,
                    p.Employee.LastName,
                    p.CashDesk.Number,
                    p.PaymentType.ToString(),
                    p.PaymentItems.Sum(i => i.Price)
                ))
                .ToList();

            return Ok(result);
        }

        // BEREITS VORHANDEN:
        [HttpGet("{id}")]
        public ActionResult<PaymentDetailDto> GetPayment(int id)
        {
            var data = _db.Payments
                .Where(p => p.Id == id)
                .Select(p =>
                    new PaymentDetailDto(
                        p.Id,
                        p.Employee.FirstName,
                        p.Employee.LastName,
                        p.CashDesk.Number,
                        p.PaymentType.ToString(),
                        p.PaymentItems.Select(i =>
                            new PaymentItemDto(
                                i.ArticleName,
                                i.Amount,
                                i.Price
                            )
                        ).ToList()
                    )
                ).FirstOrDefault();

            if (data is null) return NotFound();
            return Ok(data);
        }

        // NEU: POST /api/payments
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] NewPaymentCommand command)
        {
            // 1) Datum darf nicht mehr als 1 Minute in der Zukunft liegen
            if (command.PaymentDateTime > DateTime.Now.AddMinutes(1))
            {
                return BadRequest("Payment date time cannot be more than 1 minute in the future.");
            }

            // 2) CashDesk suchen
            var cashDesk = await _db.CashDesks.FirstOrDefaultAsync(c => c.Number == command.CashDeskNumber);
            if (cashDesk == null)
            {
                return BadRequest("Cash desk not found.");
            }

            // 3) Employee suchen
            var employee = await _db.Employees
                .FirstOrDefaultAsync(e => e.RegistrationNumber == command.EmployeeRegistrationNumber);
            if (employee == null)
            {
                return BadRequest("Employee not found.");
            }

            // 4) PaymentType (Enum) parsen
            if (!Enum.TryParse<PaymentType>(command.PaymentType, true, out var paymentType))
            {
                return BadRequest("Payment type not recognized.");
            }

            // 5) Neues Payment erstellen
            var payment = new Payment
            {
                CashDesk = cashDesk,
                PaymentDateTime = command.PaymentDateTime,
                PaymentType = paymentType,
                Employee = employee,
                PaymentItems = new List<PaymentItem>() // ggf. leer
            };

            // 6) Speichern
            try
            {
                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Hier könntest du loggen
                return StatusCode(500, $"Error while saving Payment: {ex.Message}");
            }

           
            return CreatedAtAction(
                nameof(GetPayment),
                new { id = payment.Id },
                new { payment.Id }
            );
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id, [FromQuery] bool deleteItems = false)
        {
           
            var payment = await _db.Payments
                .Include(p => p.PaymentItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            
            if (!deleteItems && payment.PaymentItems.Any())
            {
                return BadRequest("Payment has payment items.");
            }

           
            try
            {
                if (deleteItems)
                {
                   
                    _db.PaymentItems.RemoveRange(payment.PaymentItems);
                }
                // dann Payment löschen
                _db.Payments.Remove(payment);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error while deleting Payment: {ex.Message}");
            }

            
            return NoContent();
        }
    }



}
