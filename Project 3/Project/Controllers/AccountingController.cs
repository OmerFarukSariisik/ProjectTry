using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System.Data.SqlClient;
using Project.Services;

namespace Project.Controllers
{
    public class AccountingController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IProformaInvoiceService _proformaInvoiceService;
        
        private readonly string _connectionString;
        
        private bool _isInitialized;

        public AccountingController(IConfiguration configuration, IProformaInvoiceService proformaInvoiceService,
            IInvoiceService invoiceService) 
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _proformaInvoiceService = proformaInvoiceService;
            _invoiceService = invoiceService;
        }

        public IActionResult Index(string str, bool first)
        {
            _invoiceService.Initialize();
            if(string.IsNullOrEmpty(str))
                return View(_invoiceService.AllInvoices.ToArray());
            
            var filteredInvoices = new List<InvoiceModel>();
            foreach (var invoiceModel in _invoiceService.AllInvoices)
            {
                if (first && invoiceModel.InvoiceNumber.ToString().Contains(str))
                    filteredInvoices.Add(invoiceModel);
                if (!first && invoiceModel.ProformaInvoice.Customer.FullName.Contains(str))
                    filteredInvoices.Add(invoiceModel);
            }

            return View(filteredInvoices.ToArray());
        }

        public IActionResult CreateInvoicePage(int proformaInvoiceId)
        {
            _proformaInvoiceService.Initialize();
            var proformaInvoiceModel = _proformaInvoiceService.AllProformaInvoices.Find(c =>
                c.ProformaInvoiceId == proformaInvoiceId);
            
            var invoiceModel = new InvoiceModel
            {
                ProformaInvoice = proformaInvoiceModel,
                ProformaInvoiceId = proformaInvoiceModel.ProformaInvoiceId,
                InvoiceDate = DateTime.Now,
                Descriptions = proformaInvoiceModel.Descriptions,
                UnitPrices = proformaInvoiceModel.UnitPrices,
                Qtys = proformaInvoiceModel.Qtys,
                SubTotals = proformaInvoiceModel.SubTotals,
                TaxAmounts = proformaInvoiceModel.TaxAmounts,
                Totals = proformaInvoiceModel.Totals,
                SubTotal = proformaInvoiceModel.SubTotal,
                TotalTaxAmount = proformaInvoiceModel.TotalTaxAmount,
                GrandTotal = proformaInvoiceModel.GrandTotal,
                Items = proformaInvoiceModel.Items,
                AmountString = ConvertToWords((decimal)proformaInvoiceModel.GrandTotal),
                CreatedBy = proformaInvoiceModel.CreatedBy,
                Remarks = ""
            };
            return Create(invoiceModel);
        }

        public IActionResult EditPage(int targetInvoiceId)
        {
            _invoiceService.Initialize();
            var retrievedBill = _invoiceService.AllInvoices.Find(b => b.InvoiceId == targetInvoiceId);
            return View(retrievedBill);
        }

        [HttpPost]
        public IActionResult Create(InvoiceModel invoiceModel)
        {
            InvoiceModel newInvoiceModel = new InvoiceModel
            {
                ProformaInvoiceId = invoiceModel.ProformaInvoiceId,
                InvoiceNumber = invoiceModel.InvoiceNumber,
                InvoiceDate = invoiceModel.InvoiceDate,
                SubTotal = invoiceModel.SubTotal,
                TotalTaxAmount = invoiceModel.TotalTaxAmount,
                GrandTotal = invoiceModel.GrandTotal,
                CreatedBy = invoiceModel.CreatedBy,
                Remarks = invoiceModel.Remarks,
                Items = invoiceModel.Items
            };
            
            for (var i = 0; i < newInvoiceModel.Items.Length; i++)
            {
                newInvoiceModel.Descriptions += newInvoiceModel.Items[i].Description;
                newInvoiceModel.UnitPrices += newInvoiceModel.Items[i].UnitPrice;
                newInvoiceModel.Qtys += newInvoiceModel.Items[i].Qty;
                newInvoiceModel.SubTotals += newInvoiceModel.Items[i].SubTotal;
                newInvoiceModel.TaxAmounts += newInvoiceModel.Items[i].TaxAmount;
                newInvoiceModel.Totals += newInvoiceModel.Items[i].Total;
            
                if (i != invoiceModel.Items.Length - 1)
                {
                    newInvoiceModel.Descriptions += "|";
                    newInvoiceModel.UnitPrices += "|";
                    newInvoiceModel.Qtys += "|";
                    newInvoiceModel.SubTotals += "|";
                    newInvoiceModel.TaxAmounts += "|";
                    newInvoiceModel.Totals += "|";
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql =
                    "INSERT INTO invoices (ProformaInvoiceId, InvoiceNumber, InvoiceDate, Descriptions, UnitPrices, Qtys, SubTotals, TaxAmounts, Totals, SubTotal, TotalTaxAmount, GrandTotal, CreatedBy, Remarks) " +
                    "VALUES (@ProformaInvoiceId, @InvoiceNumber, @InvoiceDate, @Descriptions, @UnitPrices, @Qtys, @SubTotals, @TaxAmounts, @Totals, @SubTotal, @TotalTaxAmount, @GrandTotal, @CreatedBy, @Remarks)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ProformaInvoiceId", newInvoiceModel.ProformaInvoiceId);
                    command.Parameters.AddWithValue("@InvoiceNumber", newInvoiceModel.InvoiceNumber);
                    command.Parameters.AddWithValue("@InvoiceDate", newInvoiceModel.InvoiceDate);
                    command.Parameters.AddWithValue("@Descriptions", newInvoiceModel.Descriptions);
                    command.Parameters.AddWithValue("@UnitPrices", newInvoiceModel.UnitPrices);
                    command.Parameters.AddWithValue("@Qtys", newInvoiceModel.Qtys);
                    command.Parameters.AddWithValue("@SubTotals", newInvoiceModel.SubTotals);
                    command.Parameters.AddWithValue("@TaxAmounts", newInvoiceModel.TaxAmounts);
                    command.Parameters.AddWithValue("@Totals", newInvoiceModel.Totals);
                    command.Parameters.AddWithValue("@SubTotal", newInvoiceModel.SubTotal);
                    command.Parameters.AddWithValue("@TotalTaxAmount", newInvoiceModel.TotalTaxAmount);
                    command.Parameters.AddWithValue("@GrandTotal", newInvoiceModel.GrandTotal);
                    command.Parameters.AddWithValue("@CreatedBy", newInvoiceModel.CreatedBy);
                    command.Parameters.AddWithValue("@Remarks", newInvoiceModel.Remarks);
                    
                    command.ExecuteNonQuery();
                }
            }
            
            _invoiceService.AllInvoices.Add(newInvoiceModel);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(InvoiceModel invoiceModel)
        {
            InvoiceModel newInvoiceModel = new InvoiceModel
            {
                ProformaInvoiceId = invoiceModel.ProformaInvoiceId,
                InvoiceNumber = invoiceModel.InvoiceNumber,
                InvoiceDate = invoiceModel.InvoiceDate,
                Descriptions = invoiceModel.Descriptions,
                UnitPrices = invoiceModel.UnitPrices,
                Qtys = invoiceModel.Qtys,
                SubTotals = invoiceModel.SubTotals,
                TaxAmounts = invoiceModel.TaxAmounts,
                Totals = invoiceModel.Totals,
                SubTotal = invoiceModel.SubTotal,
                TotalTaxAmount = invoiceModel.TotalTaxAmount,
                GrandTotal = invoiceModel.GrandTotal,
                CreatedBy = invoiceModel.CreatedBy,
                Remarks = invoiceModel.Remarks
            };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "UPDATE invoices SET ProformaInvoiceId = @ProformaInvoiceId, " +
                "InvoiceNumber = @InvoiceNumber, " +
                "InvoiceDate = @InvoiceDate, " +
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
                "Remarks = @Remarks " +
                "WHERE InvoiceId = @InvoiceId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceId", newInvoiceModel.InvoiceId);
                    command.Parameters.AddWithValue("@ProformaInvoiceId", newInvoiceModel.ProformaInvoiceId);
                    command.Parameters.AddWithValue("@InvoiceNumber", newInvoiceModel.InvoiceNumber);
                    command.Parameters.AddWithValue("@InvoiceDate", newInvoiceModel.InvoiceDate);
                    command.Parameters.AddWithValue("@Descriptions", newInvoiceModel.Descriptions);
                    command.Parameters.AddWithValue("@UnitPrices", newInvoiceModel.UnitPrices);
                    command.Parameters.AddWithValue("@Qtys", newInvoiceModel.Qtys);
                    command.Parameters.AddWithValue("@SubTotals", newInvoiceModel.SubTotals);
                    command.Parameters.AddWithValue("@TaxAmounts", newInvoiceModel.TaxAmounts);
                    command.Parameters.AddWithValue("@Totals", newInvoiceModel.Totals);
                    command.Parameters.AddWithValue("@SubTotal", newInvoiceModel.SubTotal);
                    command.Parameters.AddWithValue("@TotalTaxAmount", newInvoiceModel.TotalTaxAmount);
                    command.Parameters.AddWithValue("@GrandTotal", newInvoiceModel.GrandTotal);
                    command.Parameters.AddWithValue("@CreatedBy", newInvoiceModel.CreatedBy);
                    command.Parameters.AddWithValue("@Remarks", newInvoiceModel.Remarks);
                    command.ExecuteNonQuery();
                }
            }

            _invoiceService.Initialize();
            var retrievedBill = _invoiceService.AllInvoices.Find(b => b.InvoiceId == newInvoiceModel.InvoiceId);
            var index = _invoiceService.AllInvoices.IndexOf(retrievedBill);
            _invoiceService.AllInvoices[index] = newInvoiceModel;
            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM invoices WHERE InvoiceId = @InvoiceId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceId", id);

                    command.ExecuteNonQuery();
                }
            }
            
            _invoiceService.Initialize();
            _invoiceService.AllInvoices.RemoveAll(b => b.InvoiceId == id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Search(string searchString)
        {
            /*Initialize();
            Console.WriteLine($"SearchString: {searchString} + {AllBills.Count}");
            var filteredBills = AllBills.FindAll(b => b.InvoiceNumber.Contains(searchString));
            Console.WriteLine($"FilteredBills: {filteredBills.Count}");*/
            return RedirectToAction("Index", new { str = searchString, first = true });
        }
        [HttpPost]
        public IActionResult SearchByName(string searchNameString)
        {
            /*Initialize();
            Console.WriteLine($"SearchString2: {searchNameString} + {AllBills.Count}");
            var filteredBills = AllBills.FindAll(b => b.CustomerFullName.Contains(searchNameString));
            Console.WriteLine($"FilteredBills: {filteredBills.Count}");*/
            return RedirectToAction("Index", new { str = searchNameString, first = false });
        }

        public IActionResult ShowInvoice(int id)
        {
            _invoiceService.Initialize();
            var retrievedBill = _invoiceService.AllInvoices.Find(b => b.InvoiceId == id);
            return View(retrievedBill);
        }
        
        [HttpGet]
        public string ConvertToWords(decimal amount)
        {
            return _proformaInvoiceService.ConvertToWords(amount);
        }
    }
}
