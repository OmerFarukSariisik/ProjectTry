using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentConsentLetterController : Controller
{
    private readonly IDocumentTranslationService _documentTranslationService;
    public List<DocumentConsentLetterModel> AllDocumentConsentLetter { get; set; } = new();
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentConsentLetterController(IConfiguration configuration, IDocumentTranslationService documentTranslationService)
    {
        _documentTranslationService = documentTranslationService;
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentConsentLetter') CREATE TABLE documentConsentLetter " +
                "(DocumentConsentLetterId INT PRIMARY KEY IDENTITY(1,1), Subject NVARCHAR(20) NOT NULL, OriginalLanguage NVARCHAR(20) NOT NULL, Note NVARCHAR(55)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, AppointmentDate DATE, CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _documentTranslationService.Initialize();

            string sql = "SELECT * FROM documentConsentLetter";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var documentConsentLetter = new DocumentConsentLetterModel();
                        documentConsentLetter.DocumentConsentLetterId = reader.GetInt32(0);
                        documentConsentLetter.Subject = reader.GetString(1);
                        documentConsentLetter.OriginalLanguage = reader.GetString(2);
                        documentConsentLetter.Note = reader.GetString(3);
                        
                        int columnIndex = reader.GetOrdinal("Signature");
                        long byteLength = reader.GetBytes(columnIndex, 0, null, 0, 0);
                        byte[] signatureData = new byte[byteLength];
                        reader.GetBytes(columnIndex, 0, signatureData, 0, (int)byteLength);
                        documentConsentLetter.Signature = signatureData;
                        documentConsentLetter.SignatureString = "data:image/jpeg;base64," +
                                                              Convert.ToBase64String(documentConsentLetter.Signature);

                        documentConsentLetter.CreateDate = reader.GetDateTime(5);
                        documentConsentLetter.AppointmentDate = reader.GetDateTime(6);
                        
                        documentConsentLetter.CustomerId = reader.GetInt32(7);
                        var customer = _documentTranslationService.AllCustomers.Find(c =>
                                c.CustomerId == documentConsentLetter.CustomerId);
                        
                        if(customer == null)
                            continue;
                        documentConsentLetter.FullName = customer.FullName;
                        documentConsentLetter.MotherName = customer.MotherName;
                        documentConsentLetter.Address = customer.Address;
                        documentConsentLetter.Email = customer.Email;
                        documentConsentLetter.MobileNumber = customer.MobileNumber;
                        documentConsentLetter.WhatsAppNumber = customer.WhatsAppNumber;
                        documentConsentLetter.TRNNumber = customer.TRNNumber;

                        AllDocumentConsentLetter.Add(documentConsentLetter);
                    }
                }
            }
        }
        
        _isInitialized = true;
    }
    
    public IActionResult ConsentLetterForm(DocumentConsentLetterModel DocumentConsentLetterModel)
    {
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
        
        _documentTranslationService.Initialize();
        _documentTranslationService.CreateCustomer(customer);
        
        Initialize();
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
                    _documentTranslationService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        AllDocumentConsentLetter.Add(DocumentConsentLetterModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult ConsentLetterTrackPage()
    {
        Initialize();
        return View(AllDocumentConsentLetter.ToArray());
    }
    
    public IActionResult ConsentLetterShowPage(int id)
    {
        Initialize();
        var documentPoa = AllDocumentConsentLetter.FirstOrDefault(c => c.DocumentConsentLetterId == id);
        return View(documentPoa);
    }
    
    public IActionResult Delete(int id)
    {
        Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentConsentLetter WHERE DocumentConsentLetterId = @DocumentConsentLetterId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentConsentLetterId", id);

                command.ExecuteNonQuery();
            }
        }

        var documentConsentLetter = AllDocumentConsentLetter.FirstOrDefault(c => c.DocumentConsentLetterId == id);
        AllDocumentConsentLetter.Remove(documentConsentLetter);
        return RedirectToAction("ConsentLetterTrackPage");
    }
}