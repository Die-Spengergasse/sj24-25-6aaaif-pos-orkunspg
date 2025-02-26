namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class PaymentItem
    {
        public int Id { get; set; }  // PK
        public decimal Price { get; set; }
        public int Amount { get; set; }

        public PaymentItem(decimal price, int amount)
        {
            Price = price;
            Amount = amount;
        }

        protected PaymentItem() { }
    }
}