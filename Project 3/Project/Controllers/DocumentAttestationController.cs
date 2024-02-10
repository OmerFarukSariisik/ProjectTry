using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentAttestationController : Controller
{
    private readonly IDocumentTranslationService _documentTranslationService;
    public List<DocumentAttestationModel> AllDocumentAttestations { get; set; } = new();
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentAttestationController(IConfiguration configuration, IDocumentTranslationService documentTranslationService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentAttestations') CREATE TABLE documentAttestations " +
                "(DocumentAttestationId INT PRIMARY KEY IDENTITY(1,1), OriginalLanguage NVARCHAR(20) NOT NULL, AttestationPlace NVARCHAR(20), NumberOfDocuments INT, Purpose NVARCHAR(25)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, DeliveryDate DATE, CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _documentTranslationService.Initialize();

            string sql = "SELECT * FROM documentAttestations";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var documentAttestation = new DocumentAttestationModel();
                        documentAttestation.DocumentAttestationId = reader.GetInt32(0);
                        documentAttestation.OriginalLanguage = reader.GetString(1);
                        documentAttestation.AttestationPlace = reader.GetString(2);
                        documentAttestation.NumberOfDocuments = reader.GetInt32(3);
                        documentAttestation.Purpose = reader.GetString(4);
                        
                        int columnIndex = reader.GetOrdinal("Signature");
                        long byteLength = reader.GetBytes(columnIndex, 0, null, 0, 0);
                        byte[] signatureData = new byte[byteLength];
                        reader.GetBytes(columnIndex, 0, signatureData, 0, (int)byteLength);
                        documentAttestation.Signature = signatureData;
                        documentAttestation.SignatureString = "data:image/jpeg;base64," +
                                                              Convert.ToBase64String(documentAttestation.Signature);

                        documentAttestation.CreateDate = reader.GetDateTime(6);
                        documentAttestation.DeliveryDate = reader.GetDateTime(7);
                        
                        documentAttestation.CustomerId = reader.GetInt32(8);
                        var customer = _documentTranslationService.AllCustomers.Find(c =>
                                c.CustomerId == documentAttestation.CustomerId);
                        
                        if(customer == null)
                            continue;
                        documentAttestation.FullName = customer.FullName;
                        documentAttestation.Address = customer.Address;
                        documentAttestation.Email = customer.Email;
                        documentAttestation.MobileNumber = customer.MobileNumber;
                        documentAttestation.WhatsAppNumber = customer.WhatsAppNumber;
                        documentAttestation.TRNNumber = customer.TRNNumber;

                        AllDocumentAttestations.Add(documentAttestation);
                    }
                }
            }
        }
        
        _isInitialized = true;
    }
    
    public IActionResult AttestationForm(DocumentAttestationModel documentAttestationModel)
    {
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
        
        _documentTranslationService.Initialize();
        _documentTranslationService.CreateCustomer(customer);
        
        Initialize();
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
                    _documentTranslationService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        AllDocumentAttestations.Add(documentAttestationModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult DocumentAttestationTrackPage()
    {
        Initialize();
        return View(AllDocumentAttestations.ToArray());
    }
    
    public IActionResult DocumentAttestationShowPage(int id)
    {
        Initialize();
        var documentAttestation = AllDocumentAttestations.FirstOrDefault(c => c.DocumentAttestationId == id);
        return View(documentAttestation);
    }
    
    public IActionResult Delete(int id)
    {
        Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentAttestations WHERE DocumentAttestationId = @DocumentAttestationId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentAttestationId", id);
                command.ExecuteNonQuery();
            }
        }
        var documentAttestation = AllDocumentAttestations.FirstOrDefault(c => c.DocumentAttestationId == id);
        AllDocumentAttestations.Remove(documentAttestation);
        return RedirectToAction("DocumentAttestationTrackPage");
    }
}