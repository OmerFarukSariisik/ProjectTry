using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class DocumentInvitationController : Controller
{
    private readonly IDocumentTranslationService _documentTranslationService;
    public List<DocumentInvitationModel> AllDocumentInvitation { get; set; } = new();
    private readonly string _connectionString;
    private bool _isInitialized;

    public DocumentInvitationController(IConfiguration configuration, IDocumentTranslationService documentTranslationService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'documentInvitation') CREATE TABLE documentInvitation " +
                "(DocumentInvitationId INT PRIMARY KEY IDENTITY(1,1), OriginalLanguage NVARCHAR(20) NOT NULL, Note NVARCHAR(55)" +
                ", Signature VARBINARY(MAX), CreateDate DATE, AppointmentDate DATE, CustomerId INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }
            
            _documentTranslationService.Initialize();

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
                        var customer = _documentTranslationService.AllCustomers.Find(c =>
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
    
    public IActionResult InvitationForm(DocumentInvitationModel DocumentInvitationModel)
    {
        return View(DocumentInvitationModel);
    }

    [HttpPost]
    public IActionResult CreateDocumentInvitation(DocumentInvitationModel DocumentInvitationModel)
    {
        var customer = new Customer
        {
            FullName = DocumentInvitationModel.FullName,
            MotherName = DocumentInvitationModel.MotherName,
            Address = DocumentInvitationModel.Address,
            Email = DocumentInvitationModel.Email,
            MobileNumber = DocumentInvitationModel.MobileNumber,
            WhatsAppNumber = DocumentInvitationModel.WhatsAppNumber,
            TRNNumber = ""
        };
        
        _documentTranslationService.Initialize();
        _documentTranslationService.CreateCustomer(customer);
        
        Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "INSERT INTO documentInvitation (OriginalLanguage, Note, Signature, CreateDate, AppointmentDate, CustomerId) " +
                         "VALUES (@OriginalLanguage, @Note, @Signature, @CreateDate, @AppointmentDate, @CustomerId)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@OriginalLanguage", DocumentInvitationModel.OriginalLanguage);
                command.Parameters.AddWithValue("@Note", DocumentInvitationModel.Note);
                command.Parameters.AddWithValue("@Signature",
                    Convert.FromBase64String(DocumentInvitationModel.SignatureString.Split(',')[1]));
                command.Parameters.AddWithValue("@CreateDate", DocumentInvitationModel.CreateDate);
                command.Parameters.AddWithValue("@AppointmentDate", DocumentInvitationModel.AppointmentDate);
                command.Parameters.AddWithValue("@CustomerId",
                    _documentTranslationService.AllCustomers.First(x => x.MobileNumber == customer.MobileNumber)
                        .CustomerId);

                command.ExecuteNonQuery();
            }
        }
        
        AllDocumentInvitation.Add(DocumentInvitationModel);
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult InvitationTrackPage()
    {
        Initialize();
        return View(AllDocumentInvitation.ToArray());
    }
    
    public IActionResult InvitationShowPage(int id)
    {
        Initialize();
        var documentPoa = AllDocumentInvitation.FirstOrDefault(c => c.DocumentInvitationId == id);
        return View(documentPoa);
    }
    
    public IActionResult Delete(int id)
    {
        Initialize();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM documentInvitation WHERE DocumentInvitationId = @DocumentInvitationId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DocumentInvitationId", id);

                command.ExecuteNonQuery();
            }
        }
        var documentPoa = AllDocumentInvitation.FirstOrDefault(c => c.DocumentInvitationId == id);
        AllDocumentInvitation.Remove(documentPoa);
        return RedirectToAction("InvitationTrackPage");
    }
}