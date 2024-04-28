namespace Project.Models;

public class AllDocumentsModel
{
    public DocumentAttestationModel[] DocumentAttestationModels { get; set; }
    public DocumentTranslationModel[] DocumentTranslationModels { get; set; }
    public DocumentCommitmentLetterModel[] DocumentCommitmentLetterModels { get; set; }
    public DocumentConsentLetterModel[] DocumentConsentLetterModels { get; set; }
    public DocumentInvitationModel[] DocumentInvitationModels { get; set; }
    public DocumentPoaModel[] DocumentPoaModels { get; set; }
    public DocumentSircularyModel[] DocumentSircularyModels { get; set; }
    public int CustomerId { get; set; }
}