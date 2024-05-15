using Project.Models;

namespace Project.Services;

public interface ITaxManagementService
{
    double GetThisMonthTaxAmount();
    public Dictionary<int, (double subTotal, double totalTaxAmount, double grandTotal)> GetAllYearsTaxAmounts();
    Dictionary<(int year, int month), (double subTotal, double totalTaxAmount, double grandTotal)> GetAllMonthsTaxAmounts();
    Dictionary<string, (double subTotal, double totalTaxAmount, double grandTotal)> GetAllDaysTaxAmounts();
    List<DayTaxData> GetAllDayTaxData();
}

public class TaxManagementService : ITaxManagementService
{
    private readonly IInvoiceService _invoiceService;
    private readonly string _connectionString;
    private bool _isInitialized;
    
    public TaxManagementService(IConfiguration configuration, IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public double GetThisMonthTaxAmount()
    {
        _invoiceService.Initialize();
        double totalTaxAmount = 0;
        foreach (var invoice in _invoiceService.AllInvoices)
        {
            if (invoice.InvoiceDate.Month == DateTime.Today.Month && invoice.InvoiceDate.Year == DateTime.Today.Year)
                totalTaxAmount += invoice.TotalTaxAmount;
        }
        
        return totalTaxAmount;
    }
    
    public Dictionary<(int year, int month), (double subTotal, double totalTaxAmount, double grandTotal)> GetAllMonthsTaxAmounts()
    {
        _invoiceService.Initialize();
        var monthsTaxAmounts = new Dictionary<(int year, int month), (double subTotal, double totalTaxAmount, double grandTotal)>();
        foreach (var invoice in _invoiceService.AllInvoices)
        {
            var key = (invoice.InvoiceDate.Year, invoice.InvoiceDate.Month);
            if (monthsTaxAmounts.ContainsKey(key))
            {
                var currentMonthData = monthsTaxAmounts[key];
                monthsTaxAmounts[key] = (
                    currentMonthData.subTotal + invoice.SubTotal,
                    currentMonthData.totalTaxAmount + invoice.TotalTaxAmount,
                    currentMonthData.grandTotal + invoice.GrandTotal
                );
            }
            else
            {
                monthsTaxAmounts[key] = (
                    invoice.SubTotal,
                    invoice.TotalTaxAmount,
                    invoice.GrandTotal
                );
            }
        }
    
        return monthsTaxAmounts;
    }

    public Dictionary<int, (double subTotal, double totalTaxAmount, double grandTotal)> GetAllYearsTaxAmounts()
    {
        _invoiceService.Initialize();
        var yearsTaxAmounts = new Dictionary<int, (double subTotal, double totalTaxAmount, double grandTotal)>();
        foreach (var invoice in _invoiceService.AllInvoices)
        {
            var key = invoice.InvoiceDate.Year;
            if (yearsTaxAmounts.ContainsKey(key))
            {
                var currentYearData = yearsTaxAmounts[key];
                yearsTaxAmounts[key] = (
                    currentYearData.subTotal + invoice.SubTotal,
                    currentYearData.totalTaxAmount + invoice.TotalTaxAmount,
                    currentYearData.grandTotal + invoice.GrandTotal
                );
            }
            else
            {
                yearsTaxAmounts[key] = (
                    invoice.SubTotal,
                    invoice.TotalTaxAmount,
                    invoice.GrandTotal
                );
            }
        }
    
        return yearsTaxAmounts;
    }

    public Dictionary<string, (double subTotal, double totalTaxAmount, double grandTotal)> GetAllDaysTaxAmounts()
    {
        _invoiceService.Initialize();
        var daysTaxAmounts = new Dictionary<string, (double subTotal, double totalTaxAmount, double grandTotal)>();
        foreach (var invoice in _invoiceService.AllInvoices)
        {
            var key = invoice.InvoiceDate.ToString("yyyy-MM-dd");
            if (daysTaxAmounts.ContainsKey(key))
            {
                var currentDayData = daysTaxAmounts[key];
                daysTaxAmounts[key] = (
                    currentDayData.subTotal + invoice.SubTotal,
                    currentDayData.totalTaxAmount + invoice.TotalTaxAmount,
                    currentDayData.grandTotal + invoice.GrandTotal
                );
            }
            else
            {
                daysTaxAmounts[key] = (
                    invoice.SubTotal,
                    invoice.TotalTaxAmount,
                    invoice.GrandTotal
                );
            }
        }
    
        return daysTaxAmounts;
    }
    
    public List<DayTaxData> GetAllDayTaxData()
    {
        _invoiceService.Initialize();
        var dayTaxData = new List<DayTaxData>();
        foreach (var invoice in _invoiceService.AllInvoices)
        {
            var dayData = new DayTaxData
            {
                Date = invoice.InvoiceDate,
                SubTotal = invoice.SubTotal,
                TotalTaxAmount = invoice.TotalTaxAmount,
                GrandTotal = invoice.GrandTotal
            };
            dayTaxData.Add(dayData);
        }
    
        return dayTaxData;
    }
}