namespace Project.Models;

public class DocumentSircularyModel
{
    public int CustomerId { get; set; }
    public string FullName{ get; set; }
    public string MotherName{ get; set; }
    public string Address{ get; set; }
    public string Email{ get; set; }
    public string MobileNumber{ get; set; }
    public string WhatsAppNumber{ get; set; }
    public string TRNNumber{ get; set; }
    
    public int DocumentSircularyId { get; set; }
    public string OriginalLanguage { get; set; }
    public string Note { get; set; }
    public byte[] Signature { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime AppointmentDate { get; set; }
    
    public string SignatureString { get; set; }
}