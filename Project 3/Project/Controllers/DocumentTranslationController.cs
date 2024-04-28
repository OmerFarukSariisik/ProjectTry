using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentTranslationController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IDocumentTranslationService _documentTranslationService;
    
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentTranslationController(IConfiguration configuration, ICustomerService customerService,
        IDocumentTranslationService documentTranslationService)
    {
        _documentTranslationService = documentTranslationService;
        _customerService = customerService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult TranslationForm(DocumentTranslationModel documentTranslationModel)
    {
        documentTranslationModel.CreateDate = DateTime.Now;
        return View(documentTranslationModel);
    }

    [HttpPost]
    public IActionResult CreateDocumentTranslation(DocumentTranslationModel documentTranslationModel)
    {
        var customer = new Customer
        {
            FullName = documentTranslationModel.FullName,
            Address = documentTranslationModel.Address,
            Email = documentTranslationModel.Email,
            MobileNumber = documentTranslationModel.MobileNumber,
            WhatsAppNumber = documentTranslationModel.WhatsAppNumber,
            TRNNumber = ""
        };
        
        _customerService.Initialize();
        _customerService.CreateCustomer(customer);
        
        _documentTranslationService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "INSERT INTO documentTranslations (OriginalLanguage, TranslatedLanguage, RequiredCopies, Purpose, Signature, CreateDate, DeliveryDate, AttestationService, CustomerId) " +
                         "VALUES (@OriginalLanguage, @TranslatedLanguage, @RequiredCopies, @Purpose, @Signature, @CreateDate, @DeliveryDate, @AttestationService, @CustomerId)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@OriginalLanguage", documentTranslationModel.OriginalLanguage);
                command.Parameters.AddWithValue("@TranslatedLanguage", documentTranslationModel.TranslatedLanguage);
                command.Parameters.AddWithValue("@RequiredCopies", documentTranslationModel.RequiredCopies);
                command.Parameters.AddWithValue("@Purpose", documentTranslationModel.Purpose);
                command.Parameters.AddWithValue("@Signature",
                    Convert.FromBase64String(documentTranslationModel.SignatureString.Split(',')[1]));
                command.Parameters.AddWithValue("@CreateDate", documentTranslationModel.CreateDate);
                command.Parameters.AddWithValue("@DeliveryDate", documentTranslationModel.DeliveryDate);
                command.Parameters.AddWithValue("@AttestationService", documentTranslationModel.AttestationService);
                command.Parameters.AddWithValue("@CustomerId",
                    _customerService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        _documentTranslationService.AllDocumentTranslations.Add(documentTranslationModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult DocumentTranslationTrackPage()
    {
        _documentTranslationService.Initialize();
        return View(_documentTranslationService.AllDocumentTranslations.ToArray());
    }
    
    public IActionResult DocumentTranslationShowPage(int id)
    {
        _documentTranslationService.Initialize();
        var documentTranslation = _documentTranslationService.AllDocumentTranslations.FirstOrDefault(c => c.DocumentTranslationId == id);
        return View(documentTranslation);
    }
    
    public IActionResult Delete(int proformaInvoiceId)
    {
        _documentTranslationService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentTranslations WHERE DocumentTranslationId = @DocumentTranslationId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentTranslationId", proformaInvoiceId);

                command.ExecuteNonQuery();
            }
        }
        var documentTranslation = _documentTranslationService.AllDocumentTranslations.FirstOrDefault(c => c.DocumentTranslationId == proformaInvoiceId);
        _documentTranslationService.AllDocumentTranslations.Remove(documentTranslation);
        return RedirectToAction("DocumentTranslationTrackPage");
    }
}