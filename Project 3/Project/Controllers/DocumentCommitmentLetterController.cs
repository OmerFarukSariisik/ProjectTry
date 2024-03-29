using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentCommitmentLetterController : Controller
{
    private readonly ICustomerService _customerService;
    public List<DocumentCommitmentLetterModel> AllDocumentCommitmentLetter { get; set; } = new();
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentCommitmentLetterController(IConfiguration configuration, ICustomerService customerService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentCommitmentLetter') CREATE TABLE documentCommitmentLetter " +
                "(DocumentCommitmentLetterId INT PRIMARY KEY IDENTITY(1,1), Subject NVARCHAR(20) NOT NULL, OriginalLanguage NVARCHAR(20) NOT NULL, Note NVARCHAR(55)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, AppointmentDate DATE, CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _customerService.Initialize();

            string sql = "SELECT * FROM documentCommitmentLetter";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var documentCommitmentLetter = new DocumentCommitmentLetterModel();
                        documentCommitmentLetter.DocumentCommitmentLetterId = reader.GetInt32(0);
                        documentCommitmentLetter.Subject = reader.GetString(1);
                        documentCommitmentLetter.OriginalLanguage = reader.GetString(2);
                        documentCommitmentLetter.Note = reader.GetString(3);
                        
                        int columnIndex = reader.GetOrdinal("Signature");
                        long byteLength = reader.GetBytes(columnIndex, 0, null, 0, 0);
                        byte[] signatureData = new byte[byteLength];
                        reader.GetBytes(columnIndex, 0, signatureData, 0, (int)byteLength);
                        documentCommitmentLetter.Signature = signatureData;
                        documentCommitmentLetter.SignatureString = "data:image/jpeg;base64," +
                                                              Convert.ToBase64String(documentCommitmentLetter.Signature);

                        documentCommitmentLetter.CreateDate = reader.GetDateTime(5);
                        documentCommitmentLetter.AppointmentDate = reader.GetDateTime(6);
                        
                        documentCommitmentLetter.CustomerId = reader.GetInt32(7);
                        var customer = _customerService.AllCustomers.Find(c =>
                                c.CustomerId == documentCommitmentLetter.CustomerId);
                        
                        if(customer == null)
                            continue;
                        documentCommitmentLetter.FullName = customer.FullName;
                        documentCommitmentLetter.MotherName = customer.MotherName;
                        documentCommitmentLetter.Address = customer.Address;
                        documentCommitmentLetter.Email = customer.Email;
                        documentCommitmentLetter.MobileNumber = customer.MobileNumber;
                        documentCommitmentLetter.WhatsAppNumber = customer.WhatsAppNumber;
                        documentCommitmentLetter.TRNNumber = customer.TRNNumber;

                        AllDocumentCommitmentLetter.Add(documentCommitmentLetter);
                    }
                }
            }
        }
        
        _isInitialized = true;
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
        
        Initialize();
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
        
        AllDocumentCommitmentLetter.Add(DocumentCommitmentLetterModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult CommitmentLetterTrackPage()
    {
        Initialize();
        return View(AllDocumentCommitmentLetter.ToArray());
    }
    
    public IActionResult CommitmentLetterShowPage(int id)
    {
        Initialize();
        var documentPoa = AllDocumentCommitmentLetter.FirstOrDefault(c => c.DocumentCommitmentLetterId == id);
        return View(documentPoa);
    }
    
    public IActionResult Delete(int proformaInvoiceId)
    {
        Initialize();
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

        var documentCommitmentLetter = AllDocumentCommitmentLetter.FirstOrDefault(c => c.DocumentCommitmentLetterId == proformaInvoiceId);
        AllDocumentCommitmentLetter.Remove(documentCommitmentLetter);
        return RedirectToAction("CommitmentLetterTrackPage");
    }
}