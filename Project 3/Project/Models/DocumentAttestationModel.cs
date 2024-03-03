using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class DocumentAttestationModel
{
    public int CustomerId { get; set; }
    public string FullName{ get; set; }
    public string Address{ get; set; }
    public string Email{ get; set; }
    public string MobileNumber{ get; set; }
    public string WhatsAppNumber{ get; set; }
    public string TRNNumber{ get; set; }
    
    //[Key]
    public int DocumentAttestationId { get; set; }
    public string OriginalLanguage { get; set; }
    public string AttestationPlace { get; set; }
    public int NumberOfDocuments { get; set; }
    public string Purpose { get; set; }
    public byte[] Signature { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    
    public string SignatureString { get; set; }
}