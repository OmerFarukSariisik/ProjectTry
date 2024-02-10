using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class ProformaInvoiceController : Controller
{
    private readonly IDocumentTranslationService _documentTranslationService;
    private readonly IProformaInvoiceService _proformaInvoiceService;
    private readonly string _connectionString;

    public ProformaInvoiceController(IConfiguration configuration,
        IDocumentTranslationService documentTranslationService, IProformaInvoiceService proformaInvoiceService)
    {
        _documentTranslationService = documentTranslationService;
        _proformaInvoiceService = proformaInvoiceService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult ProformaInvoiceIndex(string str, bool first)
    {
        _proformaInvoiceService.Initialize();
        Console.WriteLine("str: " + str);
        Console.WriteLine("count: " + _proformaInvoiceService.AllProformaInvoices.Count);
        if(string.IsNullOrEmpty(str))
            return View(_proformaInvoiceService.AllProformaInvoices.ToArray());
            
        var proformaInvoiceModels = new List<ProformaInvoiceModel>();
        foreach (var proformaInvoiceModel in _proformaInvoiceService.AllProformaInvoices)
        {
            if (first && proformaInvoiceModel.ProformaInvoiceNo.ToString().Contains(str))
                proformaInvoiceModels.Add(proformaInvoiceModel);
            if(!first && proformaInvoiceModel.Customer.FullName.Contains(str))
                proformaInvoiceModels.Add(proformaInvoiceModel);
        }

        return View(proformaInvoiceModels.ToArray());
    }

    public IActionResult CreateProformaInvoicePage()
    {
        return View();
    }

    public IActionResult EditProformaInvoicePage(int id)
    {
        Console.WriteLine("id: " + id);
        _proformaInvoiceService.Initialize();
        var retrievedProformaInvoice = _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == id);
        Console.WriteLine("retrievedProformaInvoice: " + retrievedProformaInvoice.ProformaInvoiceId);
        Console.WriteLine("retrievedProformaInvoice: " + retrievedProformaInvoice.Customer.FullName);
        return View(retrievedProformaInvoice);
    }


    [HttpPost]
    public IActionResult CreateProformaInvoice(ProformaInvoiceModel proformaInvoice)
    {
        ProformaInvoiceModel newProformaInvoice = new ProformaInvoiceModel
        {
            ProformaInvoiceNo = proformaInvoice.ProformaInvoiceNo,
            Items = proformaInvoice.Items,

            SubTotal = proformaInvoice.SubTotal,
            TotalTaxAmount = proformaInvoice.TotalTaxAmount,
            GrandTotal = proformaInvoice.GrandTotal,
            CreatedBy = proformaInvoice.CreatedBy,
            InvoiceDate = proformaInvoice.InvoiceDate
        };

        for (var i = 0; i < newProformaInvoice.Items.Length; i++)
        {
            newProformaInvoice.Descriptions += newProformaInvoice.Items[i].Description;
            newProformaInvoice.UnitPrices += newProformaInvoice.Items[i].UnitPrice;
            newProformaInvoice.Qtys += newProformaInvoice.Items[i].Qty;
            newProformaInvoice.SubTotals += newProformaInvoice.Items[i].SubTotal;
            newProformaInvoice.TaxAmounts += newProformaInvoice.Items[i].TaxAmount;
            newProformaInvoice.Totals += newProformaInvoice.Items[i].Total;
            
            if (i != newProformaInvoice.Items.Length - 1)
            {
                newProformaInvoice.Descriptions += "|";
                newProformaInvoice.UnitPrices += "|";
                newProformaInvoice.Qtys += "|";
                newProformaInvoice.SubTotals += "|";
                newProformaInvoice.TaxAmounts += "|";
                newProformaInvoice.Totals += "|";
            }
        }

        _documentTranslationService.Initialize();
        newProformaInvoice.Customer =
            _documentTranslationService.AllCustomers.First(x =>
                x.MobileNumber == proformaInvoice.Customer.MobileNumber);

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql =
                "INSERT INTO proformaInvoices (CustomerId, Descriptions, UnitPrices, Qtys, SubTotals, TaxAmounts, Totals, SubTotal, TotalTaxAmount, GrandTotal, CreatedBy, InvoiceDate, ProformaInvoiceNo) " +
                "VALUES (@CustomerId, @Descriptions, @UnitPrices, @Qtys, @SubTotals, @TaxAmounts, @Totals, @SubTotal, @TotalTaxAmount, @GrandTotal, @CreatedBy, @InvoiceDate, @ProformaInvoiceNo)";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", newProformaInvoice.Customer.CustomerId);
                command.Parameters.AddWithValue("@ProformaInvoiceNo", newProformaInvoice.ProformaInvoiceNo);
                command.Parameters.AddWithValue("@Descriptions", newProformaInvoice.Descriptions);
                command.Parameters.AddWithValue("@UnitPrices", newProformaInvoice.UnitPrices);
                command.Parameters.AddWithValue("@Qtys", newProformaInvoice.Qtys);
                command.Parameters.AddWithValue("@SubTotals", newProformaInvoice.SubTotals);
                command.Parameters.AddWithValue("@TaxAmounts", newProformaInvoice.TaxAmounts);
                command.Parameters.AddWithValue("@Totals", newProformaInvoice.Totals);
                command.Parameters.AddWithValue("@SubTotal", newProformaInvoice.SubTotal);
                command.Parameters.AddWithValue("@TotalTaxAmount", newProformaInvoice.TotalTaxAmount);
                command.Parameters.AddWithValue("@GrandTotal", newProformaInvoice.GrandTotal);
                command.Parameters.AddWithValue("@CreatedBy", newProformaInvoice.CreatedBy);
                command.Parameters.AddWithValue("@InvoiceDate", newProformaInvoice.InvoiceDate);
                command.ExecuteNonQuery();
            }
        }

        _proformaInvoiceService.AllProformaInvoices.Add(newProformaInvoice);
        return RedirectToAction("ProformaInvoiceIndex");
    }


    [HttpPost]
    public IActionResult Edit(ProformaInvoiceModel proformaInvoice)
    {
        ProformaInvoiceModel newProformaInvoice = new ProformaInvoiceModel
        {
            ProformaInvoiceId = proformaInvoice.ProformaInvoiceId,
            ProformaInvoiceNo = proformaInvoice.ProformaInvoiceNo,
            Items = proformaInvoice.Items,

            SubTotal = proformaInvoice.SubTotal,
            TotalTaxAmount = proformaInvoice.TotalTaxAmount,
            GrandTotal = proformaInvoice.GrandTotal,
            CreatedBy = proformaInvoice.CreatedBy,
            InvoiceDate = proformaInvoice.InvoiceDate
        };

        for (var i = 0; i < newProformaInvoice.Items.Length; i++)
        {
            newProformaInvoice.Descriptions += newProformaInvoice.Items[i].Description;
            newProformaInvoice.UnitPrices += newProformaInvoice.Items[i].UnitPrice;
            newProformaInvoice.Qtys += newProformaInvoice.Items[i].Qty;
            newProformaInvoice.SubTotals += newProformaInvoice.Items[i].SubTotal;
            newProformaInvoice.TaxAmounts += newProformaInvoice.Items[i].TaxAmount;
            newProformaInvoice.Totals += newProformaInvoice.Items[i].Total;
            
            if (i != newProformaInvoice.Items.Length - 1)
            {
                newProformaInvoice.Descriptions += "|";
                newProformaInvoice.UnitPrices += "|";
                newProformaInvoice.Qtys += "|";
                newProformaInvoice.SubTotals += "|";
                newProformaInvoice.TaxAmounts += "|";
                newProformaInvoice.Totals += "|";
            }
        }

        _documentTranslationService.Initialize();
        newProformaInvoice.Customer =
            _documentTranslationService.AllCustomers.First(x =>
                x.MobileNumber == proformaInvoice.Customer.MobileNumber);
        newProformaInvoice.CustomerId = newProformaInvoice.Customer.CustomerId;
        
        Console.WriteLine("PROFORMAA.CustomerId: " + newProformaInvoice.Customer.CustomerId);
        Console.WriteLine("PROFORMAA.FullName: " + newProformaInvoice.Customer.FullName);

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = "UPDATE proformaInvoices SET CustomerId = @CustomerId, " +
                         "Descriptions = @Descriptions, " +
                         "UnitPrices = @UnitPrices, " +
                         "Qtys = @Qtys, " +
                         "SubTotals = @SubTotals, " +
                         "TaxAmounts = @TaxAmounts, " +
                         "Totals = @Totals, " +
                         "SubTotal = @SubTotal, " +
                         "TotalTaxAmount = @TotalTaxAmount, " +
                         "GrandTotal = @GrandTotal, " +
                         "CreatedBy = @CreatedBy, " +
                         "InvoiceDate = @InvoiceDate, " +
                         "ProformaInvoiceNo = @ProformaInvoiceNo " +
                         "WHERE ProformaInvoiceId = @ProformaInvoiceId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ProformaInvoiceId", newProformaInvoice.ProformaInvoiceId);
                command.Parameters.AddWithValue("@CustomerId", newProformaInvoice.CustomerId);
                command.Parameters.AddWithValue("@ProformaInvoiceNo", newProformaInvoice.ProformaInvoiceNo);
                command.Parameters.AddWithValue("@Descriptions", newProformaInvoice.Descriptions);
                command.Parameters.AddWithValue("@UnitPrices", newProformaInvoice.UnitPrices);
                command.Parameters.AddWithValue("@Qtys", newProformaInvoice.Qtys);
                command.Parameters.AddWithValue("@SubTotals", newProformaInvoice.SubTotals);
                command.Parameters.AddWithValue("@TaxAmounts", newProformaInvoice.TaxAmounts);
                command.Parameters.AddWithValue("@Totals", newProformaInvoice.Totals);
                command.Parameters.AddWithValue("@SubTotal", newProformaInvoice.SubTotal);
                command.Parameters.AddWithValue("@TotalTaxAmount", newProformaInvoice.TotalTaxAmount);
                command.Parameters.AddWithValue("@GrandTotal", newProformaInvoice.GrandTotal);
                command.Parameters.AddWithValue("@CreatedBy", newProformaInvoice.CreatedBy);
                command.Parameters.AddWithValue("@InvoiceDate", newProformaInvoice.InvoiceDate);
                command.ExecuteNonQuery();
            }
        }
        
        Console.WriteLine("PROFORMAAA.coount1: " + _proformaInvoiceService.AllProformaInvoices.Count);
        _proformaInvoiceService.Initialize();
        Console.WriteLine("PROFORMAAA.ProformaInvoiceIdd: " + newProformaInvoice.ProformaInvoiceId);
        Console.WriteLine("PROFORMAAA.coount2: " + _proformaInvoiceService.AllProformaInvoices.Count);
        var retrievedProformaInvoice =
            _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == newProformaInvoice.ProformaInvoiceId);
        var index = _proformaInvoiceService.AllProformaInvoices.IndexOf(retrievedProformaInvoice);
        _proformaInvoiceService.AllProformaInvoices[index] = newProformaInvoice;
        return RedirectToAction("ProformaInvoiceIndex");
    }


    public IActionResult DeleteProformaInvoice(int id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM proformaInvoices WHERE ProformaInvoiceId = @ProformaInvoiceId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ProformaInvoiceId", id);

                command.ExecuteNonQuery();
            }
        }

        _proformaInvoiceService.Initialize();
        _proformaInvoiceService.AllProformaInvoices.RemoveAll(b => b.ProformaInvoiceId == id);
        return RedirectToAction("ProformaInvoiceIndex");
    }
    
    [HttpPost]
    public IActionResult Search(string searchString)
    {
        return RedirectToAction("ProformaInvoiceIndex", new { str = searchString, first = true });
    }
    [HttpPost]
    public IActionResult SearchByName(string searchNameString)
    {
        return RedirectToAction("ProformaInvoiceIndex", new { str = searchNameString, first = false });
    }

    public IActionResult ShowProformaInvoice(int id)
    {
        _proformaInvoiceService.Initialize();
        var retrievedProformaInvoice = _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == id);
        Console.WriteLine("retrievedProformaInvoice: " + retrievedProformaInvoice.Items.Length);
        return View(retrievedProformaInvoice);
    }
    
    [HttpGet]
    public IActionResult GetCustomerByMobileNumber(string mobileNumber)
    {
        Console.WriteLine("aaaa");
        _documentTranslationService.Initialize();
        var customer = _documentTranslationService.AllCustomers.FirstOrDefault(x => x.MobileNumber == mobileNumber);

        Console.WriteLine("mobile number:" + mobileNumber + "-");
        if (customer != null)
        {
            Console.WriteLine("yess " + customer.FullName);
            return Json(new
            {
                fullName = customer.FullName,
                motherName = customer.MotherName,
                address = customer.Address,
                whatsAppNumber = customer.WhatsAppNumber,
                email = customer.Email
            });
        }

        return Json(null);
    }

    public IActionResult RequestCreatePendingPage(int id)
    {
        _proformaInvoiceService.Initialize();
        var retrievedProformaInvoice = _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == id);
        if(retrievedProformaInvoice.Customer == null)
            Console.WriteLine("CUSTOMER NULLLLLL: " + retrievedProformaInvoice.CustomerId);
        else
            Console.WriteLine("CUSTOMER FOUNDDDD: " + retrievedProformaInvoice.Customer.FullName);
        
        return RedirectToAction("CreatePendingPage", "Pending", retrievedProformaInvoice);
    }
    public IActionResult RequestCreateVoucherPage(int id)
    {
        _proformaInvoiceService.Initialize();
        var retrievedProformaInvoice = _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == id);
        if(retrievedProformaInvoice.Customer == null)
            Console.WriteLine("CUSTOMER NULLLLLL: " + retrievedProformaInvoice.CustomerId);
        else
            Console.WriteLine("CUSTOMER FOUNDDDD: " + retrievedProformaInvoice.Customer.FullName);
        
        return RedirectToAction("CreateVoucherPage", "Voucher", retrievedProformaInvoice);
    }
}