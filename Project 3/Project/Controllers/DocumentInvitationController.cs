using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentInvitationController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IDocumentInvitationService _documentInvitationService;
    
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentInvitationController(IConfiguration configuration, ICustomerService customerService,
        IDocumentInvitationService documentInvitationService)
    {
        _documentInvitationService = documentInvitationService;
        _customerService = customerService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult InvitationForm(DocumentInvitationModel DocumentInvitationModel)
    {
        DocumentInvitationModel.CreateDate = DateTime.Now;
        return View(DocumentInvitationModel);
    }

    [HttpPost]
    public IActionResult CreateDocumentInvitation(DocumentInvitationModel DocumentInvitationModel)
    {
        var customer = new Customer
        {
            FullName = DocumentInvitationModel.FullName,
            MotherName = DocumentInvitationModel.MotherName,
            Address = DocumentInvitationModel.Address,
            Email = DocumentInvitationModel.Email,
            MobileNumber = DocumentInvitationModel.MobileNumber,
            WhatsAppNumber = DocumentInvitationModel.WhatsAppNumber,
            TRNNumber = ""
        };
        
        _customerService.Initialize();
        _customerService.CreateCustomer(customer);
        
        _documentInvitationService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "INSERT INTO documentInvitation (OriginalLanguage, Note, Signature, CreateDate, AppointmentDate, CustomerId) " +
                         "VALUES (@OriginalLanguage, @Note, @Signature, @CreateDate, @AppointmentDate, @CustomerId)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@OriginalLanguage", DocumentInvitationModel.OriginalLanguage);
                command.Parameters.AddWithValue("@Note", DocumentInvitationModel.Note);
                command.Parameters.AddWithValue("@Signature",
                    Convert.FromBase64String(DocumentInvitationModel.SignatureString.Split(',')[1]));
                command.Parameters.AddWithValue("@CreateDate", DocumentInvitationModel.CreateDate);
                command.Parameters.AddWithValue("@AppointmentDate", DocumentInvitationModel.AppointmentDate);
                command.Parameters.AddWithValue("@CustomerId",
                    _customerService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        _documentInvitationService.AllDocumentInvitation.Add(DocumentInvitationModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult InvitationTrackPage()
    {
        _documentInvitationService.Initialize();
        return View(_documentInvitationService.AllDocumentInvitation.ToArray());
    }
    
    public IActionResult InvitationShowPage(int id)
    {
        _documentInvitationService.Initialize();
        var documentPoa =
            _documentInvitationService.AllDocumentInvitation.FirstOrDefault(c => c.DocumentInvitationId == id);
        return View(documentPoa);
    }
    
    public IActionResult Delete(int proformaInvoiceId)
    {
        _documentInvitationService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentInvitation WHERE DocumentInvitationId = @DocumentInvitationId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentInvitationId", proformaInvoiceId);

                command.ExecuteNonQuery();
            }
        }

        var documentPoa =
            _documentInvitationService.AllDocumentInvitation.FirstOrDefault(c =>
                c.DocumentInvitationId == proformaInvoiceId);
        _documentInvitationService.AllDocumentInvitation.Remove(documentPoa);
        return RedirectToAction("InvitationTrackPage");
    }
}