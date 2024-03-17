using System.Data.SqlClient;
using Project.Models;

namespace Project.Services;

public interface IInvoiceService
{
    List<InvoiceModel> AllInvoices { get; set; }
    void Initialize();
}

public class InvoiceService : IInvoiceService
{
    private readonly IProformaInvoiceService _proformaInvoiceService;
    public List<InvoiceModel> AllInvoices { get; set; } = new();
    
    private readonly string _connectionString;
    private bool _isInitialized;
    
    public InvoiceService(IConfiguration configuration, IProformaInvoiceService proformaInvoiceService)
    {
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

                            invoiceModel.AmountString = _proformaInvoiceService.ConvertToWords((decimal)invoiceModel.GrandTotal);
                            AllInvoices.Add(invoiceModel);
                        }
                    }
                }
            }
            
            _isInitialized = true;
        }
}