namespace Project.Models
{
    public class Bill
    {
        

        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Currency { get; set; }
        public string CustomerFullName{ get; set; }
        public string ContactNumber{ get; set; }
        public DateTime Date { get; set; }
        public DateTime DeliveryDate { get; set; }
        public byte[] Signature { get; set; }
        
        
        public string SignatureString { get; set; }
    }

    
}
