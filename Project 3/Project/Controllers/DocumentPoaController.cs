using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentPoaController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IDocumentPoaService _documentPoaService;
    public List<DocumentPoaModel> AllDocumentPoa { get; set; } = new();
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentPoaController(IConfiguration configuration, ICustomerService customerService,
        IDocumentPoaService documentPoaService)
    {
        _customerService = customerService;
        _documentPoaService = documentPoaService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult DocumentPoaForm(DocumentPoaModel documentPoaModel)
    {
        documentPoaModel.CreateDate = DateTime.Now;
        return View(documentPoaModel);
    }
    public IActionResult DocumentPoaAbuDhabiForm(DocumentPoaModel documentPoaModel)
    {
        return View(documentPoaModel);
    }

    [HttpPost]
    public IActionResult CreateDocumentPoa(DocumentPoaModel documentPoaModel)
    {
        var customer = new Customer
        {
            FullName = documentPoaModel.FullName,
            MotherName = documentPoaModel.MotherName,
            Address = documentPoaModel.Address,
            Email = documentPoaModel.Email,
            MobileNumber = documentPoaModel.MobileNumber,
            WhatsAppNumber = documentPoaModel.WhatsAppNumber,
            TRNNumber = ""
        };
        
        _customerService.Initialize();
        _customerService.CreateCustomer(customer);
        
        _documentPoaService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "INSERT INTO documentPoa (OriginalLanguage, Subject, Note, Signature, CreateDate, AppointmentDate, CustomerId) " +
                         "VALUES (@OriginalLanguage, @Subject, @Note, @Signature, @CreateDate, @AppointmentDate, @CustomerId)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@OriginalLanguage", documentPoaModel.OriginalLanguage);
                command.Parameters.AddWithValue("@Subject", documentPoaModel.Subject);
                command.Parameters.AddWithValue("@Note", documentPoaModel.Note);
                command.Parameters.AddWithValue("@Signature",
                    Convert.FromBase64String(documentPoaModel.SignatureString.Split(',')[1]));
                command.Parameters.AddWithValue("@CreateDate", documentPoaModel.CreateDate);
                command.Parameters.AddWithValue("@AppointmentDate", documentPoaModel.AppointmentDate);
                command.Parameters.AddWithValue("@CustomerId",
                    _customerService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        AllDocumentPoa.Add(documentPoaModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult DocumentPoaTrackPage()
    {
        _documentPoaService.Initialize();
        return View(AllDocumentPoa.ToArray());
    }
    
    public IActionResult DocumentPoaShowPage(int id)
    {
        _documentPoaService.Initialize();
        var documentPoa = AllDocumentPoa.FirstOrDefault(c => c.DocumentPoaId == id);
        return View(documentPoa);
    }
    
    public IActionResult Delete(int proformaInvoiceId)
    {
        _documentPoaService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentPoa WHERE DocumentPoaId = @DocumentPoaId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentPoaId", proformaInvoiceId);

                command.ExecuteNonQuery();
            }
        }
        var documentPoa = AllDocumentPoa.FirstOrDefault(c => c.DocumentPoaId == proformaInvoiceId);
        AllDocumentPoa.Remove(documentPoa);
        return RedirectToAction("DocumentPoaTrackPage");
    }
}