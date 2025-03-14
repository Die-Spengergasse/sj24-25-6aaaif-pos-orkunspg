using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe3.Dtos;

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
        [HttpGet]
        public ActionResult<PaymentDto> GetPayments([FromQuery] int? number, [FromQuery] DateTime? dateFrom)
        {
            return Ok(_db.Payments
                .Where(p => number == null || p.CashDesk.Number == number)
                .Where(p => dateFrom == null || p.PaymentDateTime >= dateFrom)
                .Select(
                        p => new PaymentDto(
                                p.Id,
                                p.Employee.FirstName,
                                p.Employee.LastName,
                                p.CashDesk.Number,
                                p.PaymentType.ToString(),
                                p.PaymentItems.Where(i => i.Payment == p).Sum(i => i.Price)
                            )
                   ));
        }

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
    }
}
