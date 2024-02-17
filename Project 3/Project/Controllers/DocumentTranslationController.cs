using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentTranslationController : Controller
{
    private readonly IDocumentTranslationService _documentTranslationService;
    public List<DocumentTranslationModel> AllDocumentTranslations { get; set; } = new();
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentTranslationController(IConfiguration configuration, IDocumentTranslationService documentTranslationService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentTranslations') CREATE TABLE documentTranslations " +
                "(DocumentTranslationId INT PRIMARY KEY IDENTITY(1,1), OriginalLanguage NVARCHAR(20) NOT NULL, TranslatedLanguage NVARCHAR(20), RequiredCopies INT, Purpose NVARCHAR(25)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, DeliveryDate DATE, AttestationService NVARCHAR(10), CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _documentTranslationService.Initialize();

            string sql = "SELECT * FROM documentTranslations";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DocumentTranslationModel documentTranslation = new DocumentTranslationModel();
                        documentTranslation.DocumentTranslationId = reader.GetInt32(0);
                        documentTranslation.OriginalLanguage = reader.GetString(1);
                        documentTranslation.TranslatedLanguage = reader.GetString(2);
                        documentTranslation.RequiredCopies = reader.GetInt32(3);
                        documentTranslation.Purpose = reader.GetString(4);
                        
                        int columnIndex = reader.GetOrdinal("Signature");
                        long byteLength = reader.GetBytes(columnIndex, 0, null, 0, 0);
                        byte[] signatureData = new byte[byteLength];
                        reader.GetBytes(columnIndex, 0, signatureData, 0, (int)byteLength);
                        documentTranslation.Signature = signatureData;
                        documentTranslation.SignatureString = "data:image/jpeg;base64," +
                                                              Convert.ToBase64String(documentTranslation.Signature);

                        documentTranslation.CreateDate = reader.GetDateTime(6);
                        documentTranslation.DeliveryDate = reader.GetDateTime(7);
                        documentTranslation.AttestationService = reader.GetString(8);
                        
                        documentTranslation.CustomerId = reader.GetInt32(9);
                        var customer = _documentTranslationService.AllCustomers.Find(c =>
                                c.CustomerId == documentTranslation.CustomerId);
                        
                        if(customer == null)
                            continue;
                        documentTranslation.FullName = customer.FullName;
                        documentTranslation.Address = customer.Address;
                        documentTranslation.Email = customer.Email;
                        documentTranslation.MobileNumber = customer.MobileNumber;
                        documentTranslation.WhatsAppNumber = customer.WhatsAppNumber;
                        documentTranslation.TRNNumber = customer.TRNNumber;

                        AllDocumentTranslations.Add(documentTranslation);
                    }
                }
            }
        }
        
        _isInitialized = true;
    }

    public IActionResult DocumentTranslationFormPage()
    {
        return View();
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
        
        _documentTranslationService.Initialize();
        _documentTranslationService.CreateCustomer(customer);
        
        Initialize();
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
                    _documentTranslationService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        AllDocumentTranslations.Add(documentTranslationModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult DocumentTranslationTrackPage()
    {
        Initialize();
        return View(AllDocumentTranslations.ToArray());
    }
    
    public IActionResult DocumentTranslationShowPage(int id)
    {
        Initialize();
        var documentTranslation = AllDocumentTranslations.FirstOrDefault(c => c.DocumentTranslationId == id);
        return View(documentTranslation);
    }
    
    public IActionResult Delete(int id)
    {
        Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentTranslations WHERE DocumentTranslationId = @DocumentTranslationId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentTranslationId", id);

                command.ExecuteNonQuery();
            }
        }
        var documentTranslation = AllDocumentTranslations.FirstOrDefault(c => c.DocumentTranslationId == id);
        AllDocumentTranslations.Remove(documentTranslation);
        return RedirectToAction("DocumentTranslationTrackPage");
    }
}