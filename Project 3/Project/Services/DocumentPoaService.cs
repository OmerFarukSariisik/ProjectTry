using System.Data.SqlClient;
using Project.Models;

namespace Project.Services;

public interface IDocumentPoaService
{
    List<DocumentPoaModel> AllDocumentPoa { get; set; }
    void Initialize();
}

public class DocumentPoaService : IDocumentPoaService
{
    private readonly ICustomerService _customerService;
    
    public List<DocumentPoaModel> AllDocumentPoa { get; set; } = new();
    
    private readonly string _connectionString;
    private bool _isInitialized;
    
    public DocumentPoaService(IConfiguration configuration, ICustomerService customerService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentPoa') CREATE TABLE documentPoa " +
                "(DocumentPoaId INT PRIMARY KEY IDENTITY(1,1), OriginalLanguage NVARCHAR(20) NOT NULL, Subject NVARCHAR(20), Note NVARCHAR(55)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, AppointmentDate DATE, CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _customerService.Initialize();

            string sql = "SELECT * FROM documentPoa";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var documentPoa = new DocumentPoaModel();
                        documentPoa.DocumentPoaId = reader.GetInt32(0);
                        documentPoa.OriginalLanguage = reader.GetString(1);
                        documentPoa.Subject = reader.GetString(2);
                        documentPoa.Note = reader.GetString(3);
                        
                        int columnIndex = reader.GetOrdinal("Signature");
                        long byteLength = reader.GetBytes(columnIndex, 0, null, 0, 0);
                        byte[] signatureData = new byte[byteLength];
                        reader.GetBytes(columnIndex, 0, signatureData, 0, (int)byteLength);
                        documentPoa.Signature = signatureData;
                        documentPoa.SignatureString = "data:image/jpeg;base64," +
                                                              Convert.ToBase64String(documentPoa.Signature);

                        documentPoa.CreateDate = reader.GetDateTime(5);
                        documentPoa.AppointmentDate = reader.GetDateTime(6);
                        
                        documentPoa.CustomerId = reader.GetInt32(7);
                        var customer = _customerService.AllCustomers.Find(c =>
                                c.CustomerId == documentPoa.CustomerId);
                        
                        if(customer == null)
                            continue;
                        documentPoa.FullName = customer.FullName;
                        documentPoa.MotherName = customer.MotherName;
                        documentPoa.Address = customer.Address;
                        documentPoa.Email = customer.Email;
                        documentPoa.MobileNumber = customer.MobileNumber;
                        documentPoa.WhatsAppNumber = customer.WhatsAppNumber;
                        documentPoa.TRNNumber = customer.TRNNumber;

                        AllDocumentPoa.Add(documentPoa);
                    }
                }
            }
        }
        
        _isInitialized = true;
    }
}