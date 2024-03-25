using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class PendingController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IProformaInvoiceService _proformaInvoiceService;

    public List<PendingModel> AllPendings { get; set; } = new();
    private readonly string _connectionString;
    private bool _isInitialized;

    public PendingController(IConfiguration configuration, ICustomerService customerService, IProformaInvoiceService proformaInvoiceService)
    {
        _customerService = customerService;
        _proformaInvoiceService = proformaInvoiceService;
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'pendings') CREATE TABLE pendings " +
                "(PendingId INT PRIMARY KEY IDENTITY(1,1), ProformaInvoiceId INT NOT NULL, Note NVARCHAR(61))";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }

            string sql = "SELECT * FROM pendings";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var pendingModel = new PendingModel();
                        pendingModel.PendingId = reader.GetInt32(0);
                        pendingModel.ProformaInvoiceId = reader.GetInt32(1);
                        pendingModel.Note = reader.GetString(2);

                        _proformaInvoiceService.Initialize();
                        var proformaInvoice = _proformaInvoiceService.AllProformaInvoices.Find(c =>
                            c.ProformaInvoiceId == pendingModel.ProformaInvoiceId);

                        if (proformaInvoice == null)
                        {
                            Console.WriteLine("proformaInvoice is null:" + pendingModel.ProformaInvoiceId);
                            continue;
                        }

                        pendingModel.ProformaInvoice = proformaInvoice;
                        AllPendings.Add(pendingModel);
                    }
                }
            }
        }

        _isInitialized = true;
    }

    public IActionResult CreatePendingPage(ProformaInvoiceModel proformaInvoiceModel)
    {
        _customerService.Initialize();
        proformaInvoiceModel.Customer = _customerService.AllCustomers.Find(c =>
            c.CustomerId == proformaInvoiceModel.CustomerId);
        if(proformaInvoiceModel.Customer == null)
            Console.WriteLine("CUSTOMER NULLLLLL2: " + proformaInvoiceModel.CustomerId);
        else
            Console.WriteLine("CUSTOMER FOUNDDDD2: " + proformaInvoiceModel.Customer.FullName);
        Console.WriteLine(proformaInvoiceModel.Descriptions);
        var pendingModel = new PendingModel
        {
            ProformaInvoice = proformaInvoiceModel,
            ProformaInvoiceId = proformaInvoiceModel.ProformaInvoiceId
        };
        return View(pendingModel);
    }

    [HttpPost]
    public IActionResult CreatePending(PendingModel pendingModel)
    {
        Console.WriteLine("prof id " + pendingModel.ProformaInvoiceId);
        Console.WriteLine("Note " + pendingModel.Note);
        
        var newPendingModel = new PendingModel
        {
            ProformaInvoiceId = pendingModel.ProformaInvoiceId,
            Note = pendingModel.Note
        };

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql =
                "INSERT INTO pendings (ProformaInvoiceId, Note) " +
                "VALUES (@ProformaInvoiceId, @Note)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ProformaInvoiceId", newPendingModel.ProformaInvoiceId);
                command.Parameters.AddWithValue("@Note", newPendingModel.Note);
                command.ExecuteNonQuery();
            }
        }

        AllPendings.Add(newPendingModel);
        return RedirectToAction("PendingIndex");
    }
    
    public IActionResult PendingIndex(string str, bool first)
    {
        Initialize();
        if (string.IsNullOrEmpty(str))
        {
            Console.WriteLine("DEFAULLTTT");
            return View(AllPendings.ToArray());
        }
            
        var pendingModels = new List<PendingModel>();
        foreach (var pendingModel in AllPendings)
        {
            if (first && pendingModel.ProformaInvoice.ProformaInvoiceNo.ToString().Contains(str))
            {
                Console.WriteLine("PROFIDDDD");
                pendingModels.Add(pendingModel);
            }

            if (!first && pendingModel.ProformaInvoice.Customer.FullName.Contains(str))
            {
                Console.WriteLine("FULLNAMEEE");
                pendingModels.Add(pendingModel);
            }
        }
        return View(pendingModels.ToArray());
    }
    
    public IActionResult DeletePending(int id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM pendings WHERE PendingId = @PendingId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PendingId", id);

                command.ExecuteNonQuery();
            }
        }
        
        AllPendings.RemoveAll(b => b.PendingId == id);
        return RedirectToAction("PendingIndex");
    }
    
    public IActionResult EditPendingPage(int id)
    {
        Console.WriteLine("id: " + id);
        Initialize();
        var retrievedPending = AllPendings.Find(b => b.PendingId == id);
        return View(retrievedPending);
    }
    
    [HttpPost]
    public IActionResult EditPending(PendingModel pendingModel)
    {
        var newPendingModel = new PendingModel
        {
            PendingId = pendingModel.PendingId,
            ProformaInvoiceId = pendingModel.ProformaInvoiceId,
            Note = pendingModel.Note
        };

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = "UPDATE pendings SET ProformaInvoiceId = @ProformaInvoiceId, " +
                         "Note = @Note " +
                         "WHERE PendingId = @PendingId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@PendingId", newPendingModel.PendingId);
                command.Parameters.AddWithValue("@ProformaInvoiceId", newPendingModel.ProformaInvoiceId);
                command.Parameters.AddWithValue("@Note", newPendingModel.Note);
                command.ExecuteNonQuery();
            }
        }
        Initialize();
        var retrievedPending =
            AllPendings.Find(b => b.PendingId == newPendingModel.PendingId);
        var index = AllPendings.IndexOf(retrievedPending);
        AllPendings[index].Note = newPendingModel.Note;
        return RedirectToAction("PendingIndex");
    }
    
    [HttpPost]
    public IActionResult Search(string searchString)
    {
        return RedirectToAction("PendingIndex", new { str = searchString, first = true });
    }
    [HttpPost]
    public IActionResult SearchByName(string searchNameString)
    {
        return RedirectToAction("PendingIndex", new { str = searchNameString, first = false });
    }
}