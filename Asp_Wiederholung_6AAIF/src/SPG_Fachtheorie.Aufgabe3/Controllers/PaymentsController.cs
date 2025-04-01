using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using SPG_Fachtheorie.Aufgabe3.Commands;
using System.Net;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _service;

        public PaymentsController(PaymentService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PaymentDto>> GetPayments([FromQuery] int? number, [FromQuery] DateTime? dateFrom)
        {
            var result = _service.Payments
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

        [HttpGet("{id}")]
        public ActionResult<PaymentDetailDto> GetPayment(int id)
        {
            var data = _service.Payments
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddPayment([FromBody] NewPaymentCommand cmd)
        {
            try
            {
                var payment = _service.CreatePayment(cmd);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, new { payment.Id });
            }
            catch (PaymentServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeletePayment(int id, [FromQuery] bool deleteItems = false)
        {
            try
            {
                _service.DeletePayment(id, deleteItems);
                return NoContent();
            }
            catch (PaymentServiceException e)
            {
                if (e.Message.Contains("not found"))
                {
                    return NotFound(e.Message);
                }
                return Problem(e.Message, statusCode: 400);
            }
        }

        [HttpPost("paymentItems")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddPaymentItem([FromBody] NewPaymentItemCommand cmd)
        {
            try
            {
                _service.AddPaymentItem(cmd);
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (PaymentServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ConfirmPayment(int id)
        {
            try
            {
                _service.ConfirmPayment(id);
                return NoContent();
            }
            catch (PaymentServiceException e)
            {
                if (e.Message.Contains("not found"))
                {
                    return NotFound(e.Message);
                }
                return Problem(e.Message, statusCode: 400);
            }
        }
        
        // PUT endpoint for PaymentItems has been removed as requested
    }
}
