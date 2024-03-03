using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System.Data.SqlClient;
using Project.Services;

namespace Project.Controllers
{
    public class AccountingController : Controller
    {
        private readonly IProformaInvoiceService _proformaInvoiceService;
        public List<InvoiceModel> AllInvoices { get; set; } = new();
        private readonly string _connectionString;
        
        private bool _isInitialized;

        public AccountingController(IConfiguration configuration, IProformaInvoiceService proformaInvoiceService) 
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _proformaInvoiceService = proformaInvoiceService;
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string tableExistsSql =
                    "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'invoices') CREATE TABLE invoices " +
                    "(InvoiceId INT PRIMARY KEY IDENTITY(1,1), ProformaInvoiceId INT, InvoiceNumber INT, InvoiceDate DATE" +
                    ", Descriptions NVARCHAR(61), UnitPrices NVARCHAR(30), Qtys NVARCHAR(10), SubTotals NVARCHAR(30)" +
                    ", TaxAmounts NVARCHAR(30), Totals NVARCHAR(30), SubTotal FLOAT, TotalTaxAmount FLOAT, GrandTotal FLOAT" +
                    ", CreatedBy NVARCHAR(30), Remarks NVARCHAR(100))";

                using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
                {
                    checkTableCommand.ExecuteNonQuery();
                }

                string sql = "SELECT * FROM invoices";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InvoiceModel invoiceModel = new InvoiceModel();
                            invoiceModel.InvoiceId = reader.GetInt32(0);
                            invoiceModel.ProformaInvoiceId = reader.GetInt32(1);
                            invoiceModel.InvoiceNumber = reader.GetInt32(2);
                            invoiceModel.InvoiceDate = reader.GetDateTime(3);
                            invoiceModel.Descriptions = reader.GetString(4);
                            invoiceModel.UnitPrices = reader.GetString(5);
                            invoiceModel.Qtys = reader.GetString(6);
                            invoiceModel.SubTotals = reader.GetString(7);
                            invoiceModel.TaxAmounts = reader.GetString(8);
                            invoiceModel.Totals = reader.GetString(9);
                            invoiceModel.SubTotal = reader.GetDouble(10);
                            invoiceModel.TotalTaxAmount = reader.GetDouble(11);
                            invoiceModel.GrandTotal = reader.GetDouble(12);
                            invoiceModel.CreatedBy = reader.GetString(13);
                            invoiceModel.Remarks = reader.GetString(14);
                            
                            _proformaInvoiceService.Initialize();
                            var proformaInvoice = _proformaInvoiceService.AllProformaInvoices.Find(c =>
                                c.ProformaInvoiceId == invoiceModel.ProformaInvoiceId);

                            if (proformaInvoice == null)
                            {
                                Console.WriteLine("proformaInvoice is null:" + invoiceModel.ProformaInvoiceId);
                                continue;
                            }
                            
                            proformaInvoice.Items = new ProformaInvoiceItem[proformaInvoice.Descriptions.Split("|").Length];
                            for (var i = 0; i < proformaInvoice.Items.Length; i++)
                            {
                                proformaInvoice.Items[i] = new ProformaInvoiceItem();
                                proformaInvoice.Items[i].Description = proformaInvoice.Descriptions.Split("|")[i];
                                proformaInvoice.Items[i].UnitPrice = double.Parse(proformaInvoice.UnitPrices.Split("|")[i]);
                                proformaInvoice.Items[i].Qty = int.Parse(proformaInvoice.Qtys.Split("|")[i]);
                                proformaInvoice.Items[i].SubTotal = double.Parse(proformaInvoice.SubTotals.Split("|")[i]);
                                proformaInvoice.Items[i].TaxAmount = double.Parse(proformaInvoice.TaxAmounts.Split("|")[i]);
                                proformaInvoice.Items[i].Total = double.Parse(proformaInvoice.Totals.Split("|")[i]);
                            }
                            
                            invoiceModel.ProformaInvoice = proformaInvoice;
                            invoiceModel.Items = new ProformaInvoiceItem[invoiceModel.Descriptions.Split("|").Length];
                            for (var i = 0; i < invoiceModel.Items.Length; i++)
                            {
                                invoiceModel.Items[i] = new ProformaInvoiceItem();
                                invoiceModel.Items[i].Description = invoiceModel.Descriptions.Split("|")[i];
                                invoiceModel.Items[i].UnitPrice = double.Parse(invoiceModel.UnitPrices.Split("|")[i]);
                                invoiceModel.Items[i].Qty = int.Parse(invoiceModel.Qtys.Split("|")[i]);
                                invoiceModel.Items[i].SubTotal = double.Parse(invoiceModel.SubTotals.Split("|")[i]);
                                invoiceModel.Items[i].TaxAmount = double.Parse(invoiceModel.TaxAmounts.Split("|")[i]);
                                invoiceModel.Items[i].Total = double.Parse(invoiceModel.Totals.Split("|")[i]);
                            }

                            invoiceModel.AmountString =  ConvertToWords((decimal)invoiceModel.GrandTotal);
                            AllInvoices.Add(invoiceModel);
                        }
                    }
                }
            }
            
            _isInitialized = true;
        }

        public IActionResult Index(string str, bool first)
        {
            Initialize();
            if(string.IsNullOrEmpty(str))
                return View(AllInvoices.ToArray());
            
            var filteredInvoices = new List<InvoiceModel>();
            foreach (var invoiceModel in AllInvoices)
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
                AmountString = ConvertToWords((decimal)proformaInvoiceModel.GrandTotal)
            };
            return View(invoiceModel);
        }

        public IActionResult EditPage(int targetInvoiceId)
        {
            Initialize();
            var retrievedBill = AllInvoices.Find(b => b.InvoiceId == targetInvoiceId);
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
            
            AllInvoices.Add(newInvoiceModel);
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

            Initialize();
            var retrievedBill = AllInvoices.Find(b => b.InvoiceId == newInvoiceModel.InvoiceId);
            var index = AllInvoices.IndexOf(retrievedBill);
            AllInvoices[index] = newInvoiceModel;
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
            
            Initialize();
            AllInvoices.RemoveAll(b => b.InvoiceId == id);
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
            Initialize();
            var retrievedBill = AllInvoices.Find(b => b.InvoiceId == id);
            return View(retrievedBill);
        }
        
        [HttpGet]
        public string ConvertToWords(decimal amount)
        {
            return _proformaInvoiceService.ConvertToWords(amount);
        }
    }
}
