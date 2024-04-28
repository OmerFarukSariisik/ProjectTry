using System.Data.SqlClient;
using Project.Models;

namespace Project.Services;

public interface IDocumentTranslationService
{
    List<DocumentTranslationModel> AllDocumentTranslations { get; set; }
    void Initialize();
}

public class DocumentTranslationService : IDocumentTranslationService
{
    private readonly ICustomerService _customerService;
    
    public List<DocumentTranslationModel> AllDocumentTranslations { get; set; } = new();
    
    private readonly string _connectionString;
    private bool _isInitialized;
    
    public DocumentTranslationService(IConfiguration configuration, ICustomerService customerService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentTranslations') CREATE TABLE documentTranslations " +
                "(DocumentTranslationId INT PRIMARY KEY IDENTITY(1,1), OriginalLanguage NVARCHAR(20) NOT NULL, TranslatedLanguage NVARCHAR(20), RequiredCopies INT, Purpose NVARCHAR(25)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, DeliveryDate DATE, AttestationService NVARCHAR(10), CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _customerService.Initialize();

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
                        var customer = _customerService.AllCustomers.Find(c =>
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
}