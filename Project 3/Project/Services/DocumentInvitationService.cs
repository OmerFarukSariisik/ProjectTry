using System.Data.SqlClient;
using Project.Models;

namespace Project.Services;

public interface IDocumentInvitationService
{
    List<DocumentInvitationModel> AllDocumentInvitation { get; set; }
    void Initialize();
}

public class DocumentInvitationService : IDocumentInvitationService
{
    private readonly ICustomerService _customerService;
    
    public List<DocumentInvitationModel> AllDocumentInvitation { get; set; } = new();
    
    private readonly string _connectionString;
    private bool _isInitialized;
    
    public DocumentInvitationService(IConfiguration configuration, ICustomerService customerService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentInvitation') CREATE TABLE documentInvitation " +
                "(DocumentInvitationId INT PRIMARY KEY IDENTITY(1,1), OriginalLanguage NVARCHAR(20) NOT NULL, Note NVARCHAR(55)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, AppointmentDate DATE, CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _customerService.Initialize();

            string sql = "SELECT * FROM documentInvitation";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var documentInvitation = new DocumentInvitationModel();
                        documentInvitation.DocumentInvitationId = reader.GetInt32(0);
                        documentInvitation.OriginalLanguage = reader.GetString(1);
                        documentInvitation.Note = reader.GetString(2);
                        
                        int columnIndex = reader.GetOrdinal("Signature");
                        long byteLength = reader.GetBytes(columnIndex, 0, null, 0, 0);
                        byte[] signatureData = new byte[byteLength];
                        reader.GetBytes(columnIndex, 0, signatureData, 0, (int)byteLength);
                        documentInvitation.Signature = signatureData;
                        documentInvitation.SignatureString = "data:image/jpeg;base64," +
                                                              Convert.ToBase64String(documentInvitation.Signature);

                        documentInvitation.CreateDate = reader.GetDateTime(4);
                        documentInvitation.AppointmentDate = reader.GetDateTime(5);
                        
                        documentInvitation.CustomerId = reader.GetInt32(6);
                        var customer = _customerService.AllCustomers.Find(c =>
                                c.CustomerId == documentInvitation.CustomerId);
                        
                        if(customer == null)
                            continue;
                        documentInvitation.FullName = customer.FullName;
                        documentInvitation.MotherName = customer.MotherName;
                        documentInvitation.Address = customer.Address;
                        documentInvitation.Email = customer.Email;
                        documentInvitation.MobileNumber = customer.MobileNumber;
                        documentInvitation.WhatsAppNumber = customer.WhatsAppNumber;
                        documentInvitation.TRNNumber = customer.TRNNumber;

                        AllDocumentInvitation.Add(documentInvitation);
                    }
                }
            }
        }
        
        _isInitialized = true;
    }
}