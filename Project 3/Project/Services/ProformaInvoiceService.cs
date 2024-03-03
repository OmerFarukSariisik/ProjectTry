using System.Data.SqlClient;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Services;

public interface IProformaInvoiceService
{
    List<ProformaInvoiceModel> AllProformaInvoices { get; set; }

    void Initialize();
    string ConvertToWords(decimal amount);
}

public class ProformaInvoiceService : IProformaInvoiceService
{
    public List<ProformaInvoiceModel> AllProformaInvoices { get; set; } = new();

    private readonly ICustomerService _customerService;
    private readonly string _connectionString;
    private bool _isInitialized;

    public ProformaInvoiceService(IConfiguration configuration, ICustomerService customerService)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'proformaInvoices') CREATE TABLE proformaInvoices " +
                "(ProformaInvoiceId INT PRIMARY KEY IDENTITY(1,1), CustomerId INT NOT NULL, Descriptions NVARCHAR(61), UnitPrices NVARCHAR(30)" +
                ", Qtys NVARCHAR(10), SubTotals NVARCHAR(30), TaxAmounts NVARCHAR(30), Totals NVARCHAR(30), SubTotal FLOAT" +
                ", TotalTaxAmount FLOAT, GrandTotal FLOAT, CreatedBy NVARCHAR(30), InvoiceDate DATE, ProformaInvoiceNo INT" +
                ", Vat INT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }

            string sql = "SELECT * FROM proformaInvoices";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var proformaInvoice = new ProformaInvoiceModel();
                        proformaInvoice.ProformaInvoiceId = reader.GetInt32(0);
                        proformaInvoice.CustomerId = reader.GetInt32(1);
                        proformaInvoice.Descriptions = reader.GetString(2);
                        proformaInvoice.UnitPrices = reader.GetString(3);
                        proformaInvoice.Qtys = reader.GetString(4);
                        proformaInvoice.SubTotals = reader.GetString(5);
                        proformaInvoice.TaxAmounts = reader.GetString(6);
                        proformaInvoice.Totals = reader.GetString(7);
                        proformaInvoice.SubTotal = reader.GetDouble(8);
                        proformaInvoice.TotalTaxAmount = reader.GetDouble(9);
                        proformaInvoice.GrandTotal = reader.GetDouble(10);
                        proformaInvoice.CreatedBy = reader.GetString(11);
                        proformaInvoice.InvoiceDate = reader.GetDateTime(12);
                        proformaInvoice.ProformaInvoiceNo = reader.GetInt32(13);
                        proformaInvoice.Vat = reader.GetInt32(14);

                        _customerService.Initialize();
                        var customer = _customerService.AllCustomers.Find(c =>
                            c.CustomerId == proformaInvoice.CustomerId);

                        if (customer == null)
                        {
                            Console.WriteLine("customer is null:" + proformaInvoice.CustomerId);
                            continue;
                        }

                        proformaInvoice.Customer = customer;
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
                        
                        proformaInvoice.AmountString =  ConvertToWords((decimal)proformaInvoice.GrandTotal);
                        AllProformaInvoices.Add(proformaInvoice);
                    }
                }
            }
        }

        _isInitialized = true;
    }

    [HttpGet]
    public string ConvertToWords(decimal amount)
    {
        int dollars = (int)amount;
        int cents = (int)((amount - dollars) * 100);

        string dollarsWords = dollars.ToWords();
        string centsWords = cents > 0 ? $"{cents.ToWords()} FILS" : "ZERO FILS";

        return $"{dollarsWords.ToUpper()} AND {centsWords.ToUpper()} ONLY";
    }
}