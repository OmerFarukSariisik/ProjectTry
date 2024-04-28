using System.Data.SqlClient;
using Project.Models;

namespace Project.Services;

public interface IDocumentAttestationService
{
    List<DocumentAttestationModel> AllDocumentAttestations { get; set; }
    void Initialize();
}

public class DocumentAttestationService : IDocumentAttestationService
{
    private readonly ICustomerService _customerService;
    
    public List<DocumentAttestationModel> AllDocumentAttestations { get; set; } = new();
    
    private readonly string _connectionString;
    private bool _isInitialized;
    
    public DocumentAttestationService(IConfiguration configuration, ICustomerService customerService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentAttestations') CREATE TABLE documentAttestations " +
                "(DocumentAttestationId INT PRIMARY KEY IDENTITY(1,1), OriginalLanguage NVARCHAR(20) NOT NULL, AttestationPlace NVARCHAR(20), NumberOfDocuments INT, Purpose NVARCHAR(25)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, DeliveryDate DATE, CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _customerService.Initialize();

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
                        var customer = _customerService.AllCustomers.Find(c =>
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
}