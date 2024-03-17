namespace Project.Services;

public interface ITaxManagementService
{
    double GetThisMonthTaxAmount();
    Dictionary<int, double> GetAllMonthsTaxAmounts();
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
    
    public Dictionary<int, double> GetAllMonthsTaxAmounts()
    {
        _invoiceService.Initialize();
        var monthsTaxAmounts = new Dictionary<int, double>();
        foreach (var invoice in _invoiceService.AllInvoices)
        {
            if (monthsTaxAmounts.ContainsKey(invoice.InvoiceDate.Month))
                monthsTaxAmounts[invoice.InvoiceDate.Month] += invoice.TotalTaxAmount;
            else
                monthsTaxAmounts[invoice.InvoiceDate.Month] = invoice.TotalTaxAmount;
        }
        
        return monthsTaxAmounts;
    }
}