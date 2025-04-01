using System;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    public class PaymentServiceException : Exception
    {
        public PaymentServiceException(string message) : base(message)
        {
        }
    }
} 