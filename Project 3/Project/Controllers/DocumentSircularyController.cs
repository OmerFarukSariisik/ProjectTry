using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentSircularyController : Controller
{
    private readonly ICustomerService _customerService;
    public List<DocumentSircularyModel> AllDocumentSirculary { get; set; } = new();
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentSircularyController(IConfiguration configuration, ICustomerService customerService)
    {
        _customerService = customerService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public void Initialize()
    {
        if (_isInitialized)
            return;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string tableExistsSql =
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentSirculary') CREATE TABLE documentSirculary " +
                "(DocumentSircularyId INT PRIMARY KEY IDENTITY(1,1), OriginalLanguage NVARCHAR(20) NOT NULL, Note NVARCHAR(55)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, AppointmentDate DATE, CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _customerService.Initialize();

            string sql = "SELECT * FROM documentSirculary";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var documentSirculary = new DocumentSircularyModel();
                        documentSirculary.DocumentSircularyId = reader.GetInt32(0);
                        documentSirculary.OriginalLanguage = reader.GetString(1);
                        documentSirculary.Note = reader.GetString(2);
                        
                        int columnIndex = reader.GetOrdinal("Signature");
                        long byteLength = reader.GetBytes(columnIndex, 0, null, 0, 0);
                        byte[] signatureData = new byte[byteLength];
                        reader.GetBytes(columnIndex, 0, signatureData, 0, (int)byteLength);
                        documentSirculary.Signature = signatureData;
                        documentSirculary.SignatureString = "data:image/jpeg;base64," +
                                                              Convert.ToBase64String(documentSirculary.Signature);

                        documentSirculary.CreateDate = reader.GetDateTime(4);
                        documentSirculary.AppointmentDate = reader.GetDateTime(5);
                        
                        documentSirculary.CustomerId = reader.GetInt32(6);
                        var customer = _customerService.AllCustomers.Find(c =>
                                c.CustomerId == documentSirculary.CustomerId);
                        
                        if(customer == null)
                            continue;
                        documentSirculary.FullName = customer.FullName;
                        documentSirculary.MotherName = customer.MotherName;
                        documentSirculary.Address = customer.Address;
                        documentSirculary.Email = customer.Email;
                        documentSirculary.MobileNumber = customer.MobileNumber;
                        documentSirculary.WhatsAppNumber = customer.WhatsAppNumber;
                        documentSirculary.TRNNumber = customer.TRNNumber;

                        AllDocumentSirculary.Add(documentSirculary);
                    }
                }
            }
        }
        
        _isInitialized = true;
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
        
        Initialize();
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
        
        AllDocumentSirculary.Add(DocumentSircularyModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult SircularyTrackPage()
    {
        Initialize();
        return View(AllDocumentSirculary.ToArray());
    }
    
    public IActionResult SircularyShowPage(int id)
    {
        Initialize();
        var documentPoa = AllDocumentSirculary.FirstOrDefault(c => c.DocumentSircularyId == id);
        return View(documentPoa);
    }
    
    public IActionResult Delete(int proformaInvoiceId)
    {
        Initialize();
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
        var documentPoa = AllDocumentSirculary.FirstOrDefault(c => c.DocumentSircularyId == proformaInvoiceId);
        AllDocumentSirculary.Remove(documentPoa);
        return RedirectToAction("SircularyTrackPage");
    }
}