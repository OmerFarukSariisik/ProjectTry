namespace Project.Models
{
    public class Voucher
    {
        public int VoucherId { get; set; }
        public int ProformaInvoiceId { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        
        public Customer Customer { get; set; }
    }
}
