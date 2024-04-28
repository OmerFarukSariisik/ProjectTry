using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Data;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentAttestationController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IDocumentAttestationService _documentAttestationService;
    //private readonly ApplicationDbContext _dbContext;
    
    private readonly string _connectionString;

    public DocumentAttestationController(IConfiguration configuration, ICustomerService customerService,
        IDocumentAttestationService documentAttestationService)
    {
        _customerService = customerService;
        _documentAttestationService = documentAttestationService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult AttestationForm(DocumentAttestationModel documentAttestationModel)
    {
        documentAttestationModel.CreateDate = DateTime.Now;
        return View(documentAttestationModel);
    }
    
    [HttpPost]
    public IActionResult CreateDocumentAttestation(DocumentAttestationModel documentAttestationModel)
    {
        var customer = new Customer
        {
            FullName = documentAttestationModel.FullName,
            Address = documentAttestationModel.Address,
            Email = documentAttestationModel.Email,
            MobileNumber = documentAttestationModel.MobileNumber,
            WhatsAppNumber = documentAttestationModel.WhatsAppNumber,
            TRNNumber = ""
        };
        
        _customerService.Initialize();
        _customerService.CreateCustomer(customer);
        
        _documentAttestationService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "INSERT INTO documentAttestations (OriginalLanguage, AttestationPlace, NumberOfDocuments, Purpose, Signature, CreateDate, DeliveryDate, CustomerId) " +
                         "VALUES (@OriginalLanguage, @AttestationPlace, @NumberOfDocuments, @Purpose, @Signature, @CreateDate, @DeliveryDate, @CustomerId)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@OriginalLanguage", documentAttestationModel.OriginalLanguage);
                command.Parameters.AddWithValue("@AttestationPlace", documentAttestationModel.AttestationPlace);
                command.Parameters.AddWithValue("@NumberOfDocuments", documentAttestationModel.NumberOfDocuments);
                command.Parameters.AddWithValue("@Purpose", documentAttestationModel.Purpose);
                command.Parameters.AddWithValue("@Signature",
                    Convert.FromBase64String(documentAttestationModel.SignatureString.Split(',')[1]));
                command.Parameters.AddWithValue("@CreateDate", documentAttestationModel.CreateDate);
                command.Parameters.AddWithValue("@DeliveryDate", documentAttestationModel.DeliveryDate);
                command.Parameters.AddWithValue("@CustomerId",
                    _customerService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber).CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        _documentAttestationService.AllDocumentAttestations.Add(documentAttestationModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult DocumentAttestationTrackPage()
    {
        _documentAttestationService.Initialize();
        return View(_documentAttestationService.AllDocumentAttestations.ToArray());
    }
    
    public IActionResult DocumentAttestationShowPage(int id)
    {
        _documentAttestationService.Initialize();
        var documentAttestation = _documentAttestationService.AllDocumentAttestations.FirstOrDefault(c => c.DocumentAttestationId == id);
        return View(documentAttestation);
    }
    
    public IActionResult Delete(int proformaInvoiceId)
    {
        _documentAttestationService.Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentAttestations WHERE DocumentAttestationId = @DocumentAttestationId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentAttestationId", proformaInvoiceId);
                command.ExecuteNonQuery();
            }
        }
        var documentAttestation = _documentAttestationService.AllDocumentAttestations.FirstOrDefault(c => c.DocumentAttestationId == proformaInvoiceId);
        _documentAttestationService.AllDocumentAttestations.Remove(documentAttestation);
        return RedirectToAction("DocumentAttestationTrackPage");
    }
}