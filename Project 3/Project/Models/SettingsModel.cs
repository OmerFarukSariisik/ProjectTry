namespace Project.Models;

public class SettingsModel
{
    public int SettingsId { get; set; }
    public int AttestationTax { get; set; }
    public int CommitmentTax { get; set; }
    public int ConsentTax { get; set; }
    public int InvitationTax { get; set; }
    public int PoaTax { get; set; }
    public int SircularyTax { get; set; }
    public int TranslationTax { get; set; }
    public string Mail { get; set; }
    public string Password { get; set; }
    
    public double AttestationPrice { get; set; }
    public double CommitmentPrice { get; set; }
    public double ConsentPrice { get; set; }
    public double InvitationPrice { get; set; }
    public double PoaPrice { get; set; }
    public double SircularyPrice { get; set; }
    public double TranslationPrice { get; set; }
}