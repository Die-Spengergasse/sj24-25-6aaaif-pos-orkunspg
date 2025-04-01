using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe3.Commands;
using System;
using System.Linq;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    public class PaymentService
    {
        private readonly AppointmentContext _db;

        public PaymentService(AppointmentContext db)
        {
            _db = db;
        }

        public IQueryable<Payment> Payments => _db.Payments;

        public Payment CreatePayment(NewPaymentCommand cmd)
        {
            // Check if there's an open payment for this cashdesk
            var openPayment = _db.Payments
                .FirstOrDefault(p => p.CashDesk.Number == cmd.CashDeskNumber && p.Confirmed == null);
            
            if (openPayment != null)
            {
                throw new PaymentServiceException("Open payment for cashdesk.");
            }

            // Find the cashdesk
            var cashDesk = _db.CashDesks.FirstOrDefault(c => c.Number == cmd.CashDeskNumber);
            if (cashDesk == null)
            {
                throw new PaymentServiceException("Cash desk not found.");
            }

            // Find the employee
            var employee = _db.Employees
                .FirstOrDefault(e => e.RegistrationNumber == cmd.EmployeeRegistrationNumber);
            if (employee == null)
            {
                throw new PaymentServiceException("Employee not found.");
            }

            // Check if PaymentType is CreditCard and employee is Manager
            if (Enum.TryParse<PaymentType>(cmd.PaymentType, true, out var paymentType))
            {
                if (paymentType == PaymentType.CreditCard && employee is not Manager)
                {
                    throw new PaymentServiceException("Insufficient rights to create a credit card payment.");
                }
            }
            else
            {
                throw new PaymentServiceException("Payment type not recognized.");
            }

            // Create new payment
            var payment = new Payment
            {
                CashDesk = cashDesk,
                PaymentDateTime = DateTime.UtcNow,
                PaymentType = paymentType,
                Employee = employee,
                PaymentItems = new List<PaymentItem>(),
                Confirmed = null
            };

            _db.Payments.Add(payment);
            _db.SaveChanges();
            return payment;
        }

        public void ConfirmPayment(int paymentId)
        {
            var payment = _db.Payments.Find(paymentId);
            
            if (payment == null)
            {
                throw new PaymentServiceException("Payment not found.");
            }

            if (payment.Confirmed != null)
            {
                throw new PaymentServiceException("Payment already confirmed.");
            }

            payment.Confirmed = DateTime.UtcNow;
            _db.SaveChanges();
        }

        public void AddPaymentItem(NewPaymentItemCommand cmd)
        {
            var payment = _db.Payments.Find(cmd.PaymentId);
            
            if (payment == null)
            {
                throw new PaymentServiceException("Payment not found.");
            }

            if (payment.Confirmed != null)
            {
                throw new PaymentServiceException("Payment already confirmed.");
            }

            var paymentItem = new PaymentItem(
                cmd.ArticleName,
                cmd.Amount,
                cmd.Price,
                payment
            );

            _db.PaymentItems.Add(paymentItem);
            _db.SaveChanges();
        }

        public void DeletePayment(int paymentId, bool deleteItems)
        {
            var payment = _db.Payments
                .Include(p => p.PaymentItems)
                .FirstOrDefault(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new PaymentServiceException("Payment not found.");
            }

            if (!deleteItems && payment.PaymentItems.Any())
            {
                throw new PaymentServiceException("Payment has payment items.");
            }

            try
            {
                if (deleteItems)
                {
                    _db.PaymentItems.RemoveRange(payment.PaymentItems);
                }
                
                _db.Payments.Remove(payment);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new PaymentServiceException($"Error while deleting Payment: {ex.Message}");
            }
        }
    }
} 