using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentSircularyController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IDocumentSircularyService _documentSircularyService;
    
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentSircularyController(IConfiguration configuration, ICustomerService customerService,
        IDocumentSircularyService documentSircularyService)
    {
        _documentSircularyService = documentSircularyService;
        _customerService = customerService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult SircularyForm(DocumentSircularyModel DocumentSircularyModel)
    {
        var newModel = new DocumentSircularyModel
        {
            CreateDate = DateTime.Now,
            FullName = DocumentSircularyModel.FullName,
            Address = DocumentSircularyModel.Address,
            Email = DocumentSircularyModel.Email,
            MobileNumber = DocumentSircularyModel.MobileNumber,
            WhatsAppNumber = DocumentSircularyModel.WhatsAppNumber
        };
        return View(newModel);
    }

    [HttpPost]
    public IActionResult CreateDocumentSirculary(DocumentSircularyModel DocumentSircularyModel)
    {
        var customer = new Customer
        {
            FullName = DocumentSircularyModel.FullName,
            MotherName = DocumentSircularyModel.MotherName,
            Address = DocumentSircularyModel.Address,
            Email = DocumentSircularyModel.Email,
            MobileNumber = DocumentSircularyModel.MobileNumber,
            WhatsAppNumber = DocumentSircularyModel.WhatsAppNumber,
            TRNNumber = ""
        };
        
        _customerService.Initialize();
        _customerService.CreateCustomer(customer);
        
        _documentSircularyService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "INSERT INTO documentSirculary (OriginalLanguage, Note, Signature, CreateDate, AppointmentDate, CustomerId) " +
                         "VALUES (@OriginalLanguage, @Note, @Signature, @CreateDate, @AppointmentDate, @CustomerId)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@OriginalLanguage", DocumentSircularyModel.OriginalLanguage);
                command.Parameters.AddWithValue("@Note", DocumentSircularyModel.Note);
                command.Parameters.AddWithValue("@Signature",
                    Convert.FromBase64String(DocumentSircularyModel.SignatureString.Split(',')[1]));
                command.Parameters.AddWithValue("@CreateDate", DocumentSircularyModel.CreateDate);
                command.Parameters.AddWithValue("@AppointmentDate", DocumentSircularyModel.AppointmentDate);
                command.Parameters.AddWithValue("@CustomerId",
                    _customerService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        _documentSircularyService.AllDocumentSirculary.Add(DocumentSircularyModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult SircularyTrackPage()
    {
        _documentSircularyService.Initialize();
        return View(_documentSircularyService.AllDocumentSirculary.ToArray());
    }
    
    public IActionResult SircularyShowPage(int id)
    {
        _documentSircularyService.Initialize();
        var documentPoa =
            _documentSircularyService.AllDocumentSirculary.FirstOrDefault(c => c.DocumentSircularyId == id);
        return View(documentPoa);
    }
    
    public IActionResult Delete(int proformaInvoiceId)
    {
        _documentSircularyService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentSirculary WHERE DocumentSircularyId = @DocumentSircularyId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentSircularyId", proformaInvoiceId);

                command.ExecuteNonQuery();
            }
        }
        var documentPoa = _documentSircularyService.AllDocumentSirculary.FirstOrDefault(c => c.DocumentSircularyId == proformaInvoiceId);
        _documentSircularyService.AllDocumentSirculary.Remove(documentPoa);
        return RedirectToAction("SircularyTrackPage");
    }
}