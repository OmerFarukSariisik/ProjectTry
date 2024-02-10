using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class CustomerJobController : Controller
{
    private readonly IDocumentTranslationService _documentTranslationService;
        
    private readonly string _connectionString;

    public CustomerJobController(IConfiguration configuration, IDocumentTranslationService documentTranslationService)
    {
        _documentTranslationService = documentTranslationService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult CreateCustomerJob()
    {
        return View("CreateCustomerJobPage");
    }

    [HttpPost]
    public IActionResult CreateCustomer(Customer customerModel)
    {
        var customer = new Customer
        {
            FullName = customerModel.FullName,
            MotherName = customerModel.MotherName,
            Address = customerModel.Address,
            Email = customerModel.Email,
            MobileNumber = customerModel.MobileNumber,
            WhatsAppNumber = customerModel.WhatsAppNumber,
            TRNNumber = ""
        };
        
        _documentTranslationService.Initialize();
        _documentTranslationService.CreateCustomer(customer);

        switch (customerModel.DocumentType)
        {
            case DocumentType.TranslationForm:
                var translationModel = new DocumentTranslationModel
                {
                    FullName = customerModel.FullName,
                    Address = customerModel.Address,
                    Email = customerModel.Email,
                    MobileNumber = customerModel.MobileNumber,
                    WhatsAppNumber = customerModel.WhatsAppNumber
                };
                return RedirectToAction("TranslationForm", "DocumentTranslation", translationModel);
            case DocumentType.AttestationForm:
                var attestationModel = new DocumentAttestationModel
                {
                    FullName = customerModel.FullName,
                    Address = customerModel.Address,
                    Email = customerModel.Email,
                    MobileNumber = customerModel.MobileNumber,
                    WhatsAppNumber = customerModel.WhatsAppNumber
                };
                return RedirectToAction("AttestationForm", "DocumentAttestation", attestationModel);
            case DocumentType.PoaDubai:
                var poaDubaiModel = new DocumentPoaModel
                {
                    FullName = customerModel.FullName,
                    Address = customerModel.Address,
                    Email = customerModel.Email,
                    MobileNumber = customerModel.MobileNumber,
                    WhatsAppNumber = customerModel.WhatsAppNumber
                };
                return RedirectToAction("DocumentPoaForm", "DocumentPoa", poaDubaiModel);
            case DocumentType.PoaAbuDhabi:
                var poaAbuDhabiModel = new DocumentPoaModel
                {
                    FullName = customerModel.FullName,
                    MotherName = customerModel.MotherName,
                    Address = customerModel.Address,
                    Email = customerModel.Email,
                    MobileNumber = customerModel.MobileNumber,
                    WhatsAppNumber = customerModel.WhatsAppNumber
                };
                return RedirectToAction("DocumentPoaAbuDhabiForm", "DocumentPoa", poaAbuDhabiModel);
            case DocumentType.SignatureSirculary:
                var signatureSircularyModel = new DocumentSircularyModel
                {
                    FullName = customerModel.FullName,
                    Address = customerModel.Address,
                    Email = customerModel.Email,
                    MobileNumber = customerModel.MobileNumber,
                    WhatsAppNumber = customerModel.WhatsAppNumber
                };
                return RedirectToAction("SircularyForm", "DocumentSirculary", signatureSircularyModel);
            case DocumentType.Invitation:
                var invitationModel = new DocumentInvitationModel
                {
                    FullName = customerModel.FullName,
                    Address = customerModel.Address,
                    Email = customerModel.Email,
                    MobileNumber = customerModel.MobileNumber,
                    WhatsAppNumber = customerModel.WhatsAppNumber
                };
                return RedirectToAction("InvitationForm", "DocumentInvitation", invitationModel);
            case DocumentType.CommitmentLetter:
                var commitmentLetterModel = new DocumentCommitmentLetterModel
                {
                    FullName = customerModel.FullName,
                    Address = customerModel.Address,
                    Email = customerModel.Email,
                    MobileNumber = customerModel.MobileNumber,
                    WhatsAppNumber = customerModel.WhatsAppNumber
                };
                return RedirectToAction("CommitmentLetterForm", "DocumentCommitmentLetter", commitmentLetterModel);
            case DocumentType.ConsentLetter:
                var consentLetterModel = new DocumentConsentLetterModel
                {
                    FullName = customerModel.FullName,
                    Address = customerModel.Address,
                    Email = customerModel.Email,
                    MobileNumber = customerModel.MobileNumber,
                    WhatsAppNumber = customerModel.WhatsAppNumber
                };
                return RedirectToAction("ConsentLetterForm", "DocumentConsentLetter", consentLetterModel);
        }
        return RedirectToAction("TranslationForm", "DocumentTranslation");
    }
    
    [HttpGet]
    public IActionResult GetCustomerByMobileNumber(string mobileNumber)
    {
        _documentTranslationService.Initialize();
        var customer = _documentTranslationService.AllCustomers.FirstOrDefault(x => x.MobileNumber == mobileNumber);

        if (customer != null)
        {
            return Json(new
            {
                fullName = customer.FullName,
                motherName = customer.MotherName,
                address = customer.Address,
                whatsAppNumber = customer.WhatsAppNumber,
                email = customer.Email
            });
        }

        return Json(null);
    }
}