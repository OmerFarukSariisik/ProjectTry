using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentCommitmentLetterController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IDocumentCommitmentLetterService _documentCommitmentLetterService;
    private readonly string _connectionString;

    public DocumentCommitmentLetterController(IConfiguration configuration, ICustomerService customerService,
        IDocumentCommitmentLetterService documentCommitmentLetterService)
    {
        _customerService = customerService;
        _documentCommitmentLetterService = documentCommitmentLetterService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult CommitmentLetterForm(DocumentCommitmentLetterModel DocumentCommitmentLetterModel)
    {
        DocumentCommitmentLetterModel.CreateDate = DateTime.Now;
        return View(DocumentCommitmentLetterModel);
    }

    [HttpPost]
    public IActionResult CreateDocumentCommitmentLetter(DocumentCommitmentLetterModel DocumentCommitmentLetterModel)
    {
        var customer = new Customer
        {
            FullName = DocumentCommitmentLetterModel.FullName,
            MotherName = DocumentCommitmentLetterModel.MotherName,
            Address = DocumentCommitmentLetterModel.Address,
            Email = DocumentCommitmentLetterModel.Email,
            MobileNumber = DocumentCommitmentLetterModel.MobileNumber,
            WhatsAppNumber = DocumentCommitmentLetterModel.WhatsAppNumber,
            TRNNumber = ""
        };
        
        _customerService.Initialize();
        _customerService.CreateCustomer(customer);
        
        _documentCommitmentLetterService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "INSERT INTO documentCommitmentLetter (Subject, OriginalLanguage, Note, Signature, CreateDate, AppointmentDate, CustomerId) " +
                         "VALUES (@Subject, @OriginalLanguage, @Note, @Signature, @CreateDate, @AppointmentDate, @CustomerId)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Subject", DocumentCommitmentLetterModel.Subject);
                command.Parameters.AddWithValue("@OriginalLanguage", DocumentCommitmentLetterModel.OriginalLanguage);
                command.Parameters.AddWithValue("@Note", DocumentCommitmentLetterModel.Note);
                command.Parameters.AddWithValue("@Signature",
                    Convert.FromBase64String(DocumentCommitmentLetterModel.SignatureString.Split(',')[1]));
                command.Parameters.AddWithValue("@CreateDate", DocumentCommitmentLetterModel.CreateDate);
                command.Parameters.AddWithValue("@AppointmentDate", DocumentCommitmentLetterModel.AppointmentDate);
                command.Parameters.AddWithValue("@CustomerId",
                    _customerService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        _documentCommitmentLetterService.AllDocumentCommitmentLetter.Add(DocumentCommitmentLetterModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult CommitmentLetterTrackPage()
    {
        _documentCommitmentLetterService.Initialize();
        return View(_documentCommitmentLetterService.AllDocumentCommitmentLetter.ToArray());
    }
    
    public IActionResult CommitmentLetterShowPage(int id)
    {
        _documentCommitmentLetterService.Initialize();
        var documentPoa = _documentCommitmentLetterService.AllDocumentCommitmentLetter.FirstOrDefault(c => c.DocumentCommitmentLetterId == id);
        return View(documentPoa);
    }
    
    public IActionResult Delete(int proformaInvoiceId)
    {
        _documentCommitmentLetterService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentCommitmentLetter WHERE DocumentCommitmentLetterId = @DocumentCommitmentLetterId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentCommitmentLetterId", proformaInvoiceId);

                command.ExecuteNonQuery();
            }
        }

        var documentCommitmentLetter = _documentCommitmentLetterService.AllDocumentCommitmentLetter.FirstOrDefault(c => c.DocumentCommitmentLetterId == proformaInvoiceId);
        _documentCommitmentLetterService.AllDocumentCommitmentLetter.Remove(documentCommitmentLetter);
        return RedirectToAction("CommitmentLetterTrackPage");
    }
}