using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class ProformaInvoiceController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IProformaInvoiceService _proformaInvoiceService;
    private readonly ISettingsService _settingsService;
    
    private readonly IDocumentAttestationService _documentAttestationService;
    private readonly IDocumentCommitmentLetterService _documentCommitmentLetterService;
    private readonly IDocumentConsentLetterService _documentConsentLetterService;
    private readonly IDocumentInvitationService _documentInvitationService;
    private readonly IDocumentPoaService _documentPoaService;
    private readonly IDocumentSircularyService _documentSircularyService;
    private readonly IDocumentTranslationService _documentTranslationService;
    
    private readonly string _connectionString;

    public ProformaInvoiceController(IConfiguration configuration,
        ICustomerService customerService, IProformaInvoiceService proformaInvoiceService, IDocumentAttestationService documentAttestationService, IDocumentCommitmentLetterService documentCommitmentLetterService, IDocumentConsentLetterService documentConsentLetterService, IDocumentInvitationService documentInvitationService, IDocumentPoaService documentPoaService, IDocumentSircularyService documentSircularyService, IDocumentTranslationService documentTranslationService, ISettingsService settingsService)
    {
        _customerService = customerService;
        _proformaInvoiceService = proformaInvoiceService;
        _documentAttestationService = documentAttestationService;
        _documentCommitmentLetterService = documentCommitmentLetterService;
        _documentConsentLetterService = documentConsentLetterService;
        _documentInvitationService = documentInvitationService;
        _documentPoaService = documentPoaService;
        _documentSircularyService = documentSircularyService;
        _documentTranslationService = documentTranslationService;
        _settingsService = settingsService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public IActionResult ProformaInvoiceIndex(string str, bool first)
    {
        _proformaInvoiceService.Initialize();
        
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

    public IActionResult ChooseCustomerForProformaInvoicePage()
    {
        _customerService.Initialize();
        return View(_customerService.AllCustomers.ToArray());
    }
    
    public IActionResult ChooseServicePage(int id)
    {
        _documentAttestationService.Initialize();
        _documentCommitmentLetterService.Initialize();
        _documentConsentLetterService.Initialize();
        _documentInvitationService.Initialize();
        _documentPoaService.Initialize();
        _documentSircularyService.Initialize();
        _documentTranslationService.Initialize();
        
        var allDocumentsModel = new AllDocumentsModel();
        allDocumentsModel.DocumentAttestationModels = _documentAttestationService.AllDocumentAttestations
            .Where(x => x.CustomerId == id).ToArray();
        allDocumentsModel.DocumentCommitmentLetterModels = _documentCommitmentLetterService.AllDocumentCommitmentLetter
            .Where(x => x.CustomerId == id).ToArray();
        allDocumentsModel.DocumentConsentLetterModels = _documentConsentLetterService.AllDocumentConsentLetter
            .Where(x => x.CustomerId == id).ToArray();
        allDocumentsModel.DocumentInvitationModels = _documentInvitationService.AllDocumentInvitation
            .Where(x => x.CustomerId == id).ToArray();
        allDocumentsModel.DocumentPoaModels =
            _documentPoaService.AllDocumentPoa.Where(x => x.CustomerId == id).ToArray();
        allDocumentsModel.DocumentSircularyModels = _documentSircularyService.AllDocumentSirculary
            .Where(x => x.CustomerId == id).ToArray();
        allDocumentsModel.DocumentTranslationModels = _documentTranslationService.AllDocumentTranslations
            .Where(x => x.CustomerId == id).ToArray();
        allDocumentsModel.CustomerId = id;
        
        return View(allDocumentsModel);
    }
    
    public IActionResult CreateProformaInvoicePage(int customerId, List<int> selectedDocuments, List<string> documentTypes)
    {
        _proformaInvoiceService.Initialize();
        _settingsService.Initialize();
        _customerService.Initialize();
        var customer = _customerService.AllCustomers.FirstOrDefault(x => x.CustomerId == customerId);
        
        var items = new ProformaInvoiceItem[selectedDocuments.Count];
        for( var i = 0; i < selectedDocuments.Count; i++)
        {
            var document = new ProformaInvoiceItem
            {
                Description = documentTypes[i],
                UnitPrice = 0,
                Qty = 1,
                SubTotal = 0
            };
            
            document.Vat = documentTypes[i] switch
            {
                "DocumentAttestation" => _settingsService.AllSettings.First().AttestationTax,
                "DocumentCommitmentLetter" => _settingsService.AllSettings.First().CommitmentTax,
                "DocumentConsentLetter" => _settingsService.AllSettings.First().ConsentTax,
                "DocumentInvitation" => _settingsService.AllSettings.First().InvitationTax,
                "DocumentPoa" => _settingsService.AllSettings.First().PoaTax,
                "DocumentSirculary" => _settingsService.AllSettings.First().SircularyTax,
                "DocumentTranslation" => _settingsService.AllSettings.First().TranslationTax,
                _ => 0 // Default case, if documentTypes[i] doesn't match any case
            };
            document.VatString = document.Vat.ToString(CultureInfo.InvariantCulture);
            
            document.UnitPrice = documentTypes[i] switch
            {
                "DocumentAttestation" => _settingsService.AllSettings.First().AttestationPrice,
                "DocumentCommitmentLetter" => _settingsService.AllSettings.First().CommitmentPrice,
                "DocumentConsentLetter" => _settingsService.AllSettings.First().ConsentPrice,
                "DocumentInvitation" => _settingsService.AllSettings.First().InvitationPrice,
                "DocumentPoa" => _settingsService.AllSettings.First().PoaPrice,
                "DocumentSirculary" => _settingsService.AllSettings.First().SircularyPrice,
                "DocumentTranslation" => _settingsService.AllSettings.First().TranslationPrice,
                _ => 0 // Default case, if documentTypes[i] doesn't match any case
            };
            document.Qty = 1;
            document.UnitPriceString = document.UnitPrice.ToString(CultureInfo.InvariantCulture);
            items[i] = document;
        }
        
        var proformaInvoice = new ProformaInvoiceModel
        {
            Customer = customer,
            CustomerId = customer.CustomerId,
            Items =  items,
            InvoiceDate = DateTime.Today
        };
        return View(proformaInvoice);
    }

    public IActionResult EditProformaInvoicePage(int proformaInvoiceId)
    {
        _proformaInvoiceService.Initialize();
        var retrievedProformaInvoice =
            _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == proformaInvoiceId);
        return View(retrievedProformaInvoice);
    }


    [HttpPost]
    public IActionResult CreateProformaInvoice(ProformaInvoiceModel proformaInvoice)
    {
        ProformaInvoiceModel newProformaInvoice = new ProformaInvoiceModel
        {
            ProformaInvoiceNo = proformaInvoice.ProformaInvoiceNo,
            Items = proformaInvoice.Items,
            SubTotal = double.Parse(proformaInvoice.SubTotalString, CultureInfo.InvariantCulture),
            TotalTaxAmount = double.Parse(proformaInvoice.TotalTaxAmountString, CultureInfo.InvariantCulture),
            GrandTotal = double.Parse(proformaInvoice.GrandTotalString, CultureInfo.InvariantCulture),
            CreatedBy = proformaInvoice.CreatedBy,
            InvoiceDate = proformaInvoice.InvoiceDate
        };

        for (var i = 0; i < newProformaInvoice.Items.Length; i++)
        {
            newProformaInvoice.Descriptions += newProformaInvoice.Items[i].Description;
            newProformaInvoice.UnitPrices += newProformaInvoice.Items[i].UnitPriceString;
            newProformaInvoice.Qtys += newProformaInvoice.Items[i].Qty;
            newProformaInvoice.SubTotals += newProformaInvoice.Items[i].SubTotalString;
            newProformaInvoice.TaxAmounts += newProformaInvoice.Items[i].TaxAmountString;
            newProformaInvoice.Totals += newProformaInvoice.Items[i].TotalString;
            newProformaInvoice.Vats += newProformaInvoice.Items[i].VatString;
            
            if (i != newProformaInvoice.Items.Length - 1)
            {
                newProformaInvoice.Descriptions += "|";
                newProformaInvoice.UnitPrices += "|";
                newProformaInvoice.Qtys += "|";
                newProformaInvoice.SubTotals += "|";
                newProformaInvoice.TaxAmounts += "|";
                newProformaInvoice.Totals += "|";
                newProformaInvoice.Vats += "|";
            }
        }

        _customerService.Initialize();
        newProformaInvoice.Customer =
            _customerService.AllCustomers.First(x =>
                x.MobileNumber == proformaInvoice.Customer.MobileNumber);

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql =
                "INSERT INTO proformaInvoices (CustomerId, Descriptions, UnitPrices, Qtys, SubTotals, TaxAmounts, Totals, SubTotal, TotalTaxAmount, GrandTotal, CreatedBy, InvoiceDate, ProformaInvoiceNo, Vats) " +
                "VALUES (@CustomerId, @Descriptions, @UnitPrices, @Qtys, @SubTotals, @TaxAmounts, @Totals, @SubTotal, @TotalTaxAmount, @GrandTotal, @CreatedBy, @InvoiceDate, @ProformaInvoiceNo, @Vats)";

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
                command.Parameters.AddWithValue("@Vats", newProformaInvoice.Vats);
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
            newProformaInvoice.Vats += newProformaInvoice.Items[i].Vat;
            
            if (i != newProformaInvoice.Items.Length - 1)
            {
                newProformaInvoice.Descriptions += "|";
                newProformaInvoice.UnitPrices += "|";
                newProformaInvoice.Qtys += "|";
                newProformaInvoice.SubTotals += "|";
                newProformaInvoice.TaxAmounts += "|";
                newProformaInvoice.Totals += "|";
                newProformaInvoice.Vats += "|";
            }
        }

        _customerService.Initialize();
        newProformaInvoice.Customer =
            _customerService.AllCustomers.First(x =>
                x.MobileNumber == proformaInvoice.Customer.MobileNumber);
        newProformaInvoice.CustomerId = newProformaInvoice.Customer.CustomerId;

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
                         "ProformaInvoiceNo = @ProformaInvoiceNo, " +
                         "Vats = @Vats " +
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
                command.Parameters.AddWithValue("@Vats", newProformaInvoice.Vats);
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
        
        _proformaInvoiceService.Initialize();
        var retrievedProformaInvoice =
            _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == newProformaInvoice.ProformaInvoiceId);
        var index = _proformaInvoiceService.AllProformaInvoices.IndexOf(retrievedProformaInvoice);
        _proformaInvoiceService.AllProformaInvoices[index] = newProformaInvoice;
        return RedirectToAction("ProformaInvoiceIndex");
    }


    public IActionResult DeleteProformaInvoice(int proformaInvoiceId)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "DELETE FROM proformaInvoices WHERE ProformaInvoiceId = @ProformaInvoiceId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ProformaInvoiceId", proformaInvoiceId);

                command.ExecuteNonQuery();
            }
        }

        _proformaInvoiceService.Initialize();
        _proformaInvoiceService.AllProformaInvoices.RemoveAll(b => b.ProformaInvoiceId == proformaInvoiceId);
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

    public IActionResult ShowProformaInvoice(int proformaInvoiceId)
    {
        _proformaInvoiceService.Initialize();
        var retrievedProformaInvoice =
            _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == proformaInvoiceId);
        return View(retrievedProformaInvoice);
    }
    
    [HttpGet]
    public IActionResult GetCustomerByMobileNumber(string mobileNumber)
    {
        _customerService.Initialize();
        var customer = _customerService.AllCustomers.FirstOrDefault(x => x.MobileNumber == mobileNumber);
        
        if (customer != null)
        {
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

    public IActionResult RequestCreatePendingPage(int proformaInvoiceId)
    {
        _proformaInvoiceService.Initialize();
        var retrievedProformaInvoice = _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == proformaInvoiceId);
        return RedirectToAction("CreatePendingPage", "Pending", retrievedProformaInvoice);
    }
    public IActionResult RequestCreateInvoicePage(int proformaInvoiceId)
    {
        return RedirectToAction("CreateInvoicePage", "Accounting", new { proformaInvoiceId = proformaInvoiceId });
    }
    public IActionResult RequestCreateVoucherPage(int proformaInvoiceId)
    {
        _proformaInvoiceService.Initialize();
        var retrievedProformaInvoice = _proformaInvoiceService.AllProformaInvoices.Find(b => b.ProformaInvoiceId == proformaInvoiceId);
        return RedirectToAction("CreateVoucherPage", "Voucher", retrievedProformaInvoice);
    }
    
    [HttpGet]
    public string ConvertToWords(decimal amount)
    {
        return _proformaInvoiceService.ConvertToWords(amount);
    }
}