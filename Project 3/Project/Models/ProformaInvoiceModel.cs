namespace Project.Models;

public class ProformaInvoiceModel
{
    public int ProformaInvoiceId { get; set; }
    public int ProformaInvoiceNo { get; set; }
    public int CustomerId { get; set; }
    
    public ProformaInvoiceItem[] Items { get; set; }
    public Customer Customer { get; set; }
    public string Descriptions { get; set; }
    public string UnitPrices { get; set; }
    public string Qtys { get; set; }
    public string SubTotals { get; set; }
    public string TaxAmounts { get; set; }
    public string Totals { get; set; }
    public string Vats { get; set; }
    
    public double SubTotal { get; set; }
    public string SubTotalString { get; set; }
    public double TotalTaxAmount { get; set; }
    public string TotalTaxAmountString { get; set; }
    public double GrandTotal { get; set; }
    public string GrandTotalString { get; set; }
    public string CreatedBy { get; set; }
    public DateTime InvoiceDate { get; set; }
    
    //
    public string AmountString { get; set; }
}

public class ProformaInvoiceItem
{
    public string Description { get; set; }
    public double UnitPrice { get; set; }
    public string UnitPriceString { get; set; }
    public int Qty { get; set; }
    public double SubTotal { get; set; }
    public string SubTotalString { get; set; }
    public double TaxAmount { get; set; }
    public string TaxAmountString { get; set; }
    public double Total { get; set; }
    public string TotalString { get; set; }
    public double Vat { get; set; }
    public string VatString { get; set; }
}