using Project.Services;

namespace Project.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FullName{ get; set; }
        public string MotherName{ get; set; }
        public string Address{ get; set; }
        public string Email{ get; set; }
        public string MobileNumber{ get; set; }
        public string WhatsAppNumber{ get; set; }
        public string TRNNumber{ get; set; }
        public string Satisfaction{ get; set; }
        public DocumentType DocumentType{ get; set; }
    }
}
