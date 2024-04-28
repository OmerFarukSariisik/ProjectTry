using System.Data.SqlClient;
using Project.Models;

namespace Project.Services;

public interface IDocumentCommitmentLetterService
{
    List<DocumentCommitmentLetterModel> AllDocumentCommitmentLetter { get; set; }
    void Initialize();
}

public class DocumentCommitmentLetterService : IDocumentCommitmentLetterService
{
    private readonly ICustomerService _customerService;
    
    public List<DocumentCommitmentLetterModel> AllDocumentCommitmentLetter { get; set; } = new();
    
    private readonly string _connectionString;
    private bool _isInitialized;
    
    public DocumentCommitmentLetterService(IConfiguration configuration, ICustomerService customerService)
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
}