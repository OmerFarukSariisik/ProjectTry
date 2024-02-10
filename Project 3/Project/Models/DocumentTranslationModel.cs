namespace Project.Models;

public class DocumentTranslationModel
{
    public int CustomerId { get; set; }
    public string FullName{ get; set; }
    public string Address{ get; set; }
    public string Email{ get; set; }
    public string MobileNumber{ get; set; }
    public string WhatsAppNumber{ get; set; }
    public string TRNNumber{ get; set; }
    
    
    public int DocumentTranslationId { get; set; }
    public string OriginalLanguage { get; set; }
    public string TranslatedLanguage { get; set; }
    public int RequiredCopies { get; set; }
    public string Purpose { get; set; }
    public byte[] Signature { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string AttestationService { get; set; }
    
    public string SignatureString { get; set; }
}