namespace Project.Models
{
    public class Voucher
    {
        public int VoucherId { get; set; }
        public int ProformaInvoiceId { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public byte[] Signature { get; set; }
        
        public Customer Customer { get; set; }
        public string SignatureString { get; set; }
        public string AmountString { get; set; }
    }
}
