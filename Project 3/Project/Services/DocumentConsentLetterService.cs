using System.Data.SqlClient;
using Project.Models;

namespace Project.Services;

public interface IDocumentConsentLetterService
{
    List<DocumentConsentLetterModel> AllDocumentConsentLetter { get; set; }
    void Initialize();
}

public class DocumentConsentLetterService : IDocumentConsentLetterService
{
    private readonly ICustomerService _customerService;
    
    public List<DocumentConsentLetterModel> AllDocumentConsentLetter { get; set; } = new();
    
    private readonly string _connectionString;
    private bool _isInitialized;
    
    public DocumentConsentLetterService(IConfiguration configuration, ICustomerService customerService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentConsentLetter') CREATE TABLE documentConsentLetter " +
                "(DocumentConsentLetterId INT PRIMARY KEY IDENTITY(1,1), Subject NVARCHAR(20) NOT NULL, OriginalLanguage NVARCHAR(20) NOT NULL, Note NVARCHAR(55)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, AppointmentDate DATE, CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _customerService.Initialize();

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
                        var customer = _customerService.AllCustomers.Find(c =>
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
}