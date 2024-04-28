using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentConsentLetterController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IDocumentConsentLetterService _documentConsentLetterService;
    
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentConsentLetterController(IConfiguration configuration, ICustomerService customerService,
        IDocumentConsentLetterService documentConsentLetterService)
    {
        _documentConsentLetterService = documentConsentLetterService;
        _customerService = customerService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult ConsentLetterForm(DocumentConsentLetterModel DocumentConsentLetterModel)
    {
        DocumentConsentLetterModel.CreateDate = DateTime.Now;
        return View(DocumentConsentLetterModel);
    }

    [HttpPost]
    public IActionResult CreateDocumentConsentLetter(DocumentConsentLetterModel DocumentConsentLetterModel)
    {
        var customer = new Customer
        {
            FullName = DocumentConsentLetterModel.FullName,
            MotherName = DocumentConsentLetterModel.MotherName,
            Address = DocumentConsentLetterModel.Address,
            Email = DocumentConsentLetterModel.Email,
            MobileNumber = DocumentConsentLetterModel.MobileNumber,
            WhatsAppNumber = DocumentConsentLetterModel.WhatsAppNumber,
            TRNNumber = ""
        };
        
        _customerService.Initialize();
        _customerService.CreateCustomer(customer);
        
        _documentConsentLetterService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "INSERT INTO documentConsentLetter (Subject, OriginalLanguage, Note, Signature, CreateDate, AppointmentDate, CustomerId) " +
                         "VALUES (@Subject, @OriginalLanguage, @Note, @Signature, @CreateDate, @AppointmentDate, @CustomerId)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Subject", DocumentConsentLetterModel.Subject);
                command.Parameters.AddWithValue("@OriginalLanguage", DocumentConsentLetterModel.OriginalLanguage);
                command.Parameters.AddWithValue("@Note", DocumentConsentLetterModel.Note);
                command.Parameters.AddWithValue("@Signature",
                    Convert.FromBase64String(DocumentConsentLetterModel.SignatureString.Split(',')[1]));
                command.Parameters.AddWithValue("@CreateDate", DocumentConsentLetterModel.CreateDate);
                command.Parameters.AddWithValue("@AppointmentDate", DocumentConsentLetterModel.AppointmentDate);
                command.Parameters.AddWithValue("@CustomerId",
                    _customerService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        _documentConsentLetterService.AllDocumentConsentLetter.Add(DocumentConsentLetterModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult ConsentLetterTrackPage()
    {
        _documentConsentLetterService.Initialize();
        return View(_documentConsentLetterService.AllDocumentConsentLetter.ToArray());
    }
    
    public IActionResult ConsentLetterShowPage(int id)
    {
        _documentConsentLetterService.Initialize();
        var documentPoa = _documentConsentLetterService.AllDocumentConsentLetter.FirstOrDefault(c => c.DocumentConsentLetterId == id);
        return View(documentPoa);
    }
    
    public IActionResult Delete(int proformaInvoiceId)
    {
        _documentConsentLetterService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentConsentLetter WHERE DocumentConsentLetterId = @DocumentConsentLetterId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentConsentLetterId", proformaInvoiceId);

                command.ExecuteNonQuery();
            }
        }

        var documentConsentLetter = _documentConsentLetterService.AllDocumentConsentLetter.FirstOrDefault(c => c.DocumentConsentLetterId == proformaInvoiceId);
        _documentConsentLetterService.AllDocumentConsentLetter.Remove(documentConsentLetter);
        return RedirectToAction("ConsentLetterTrackPage");
    }
}