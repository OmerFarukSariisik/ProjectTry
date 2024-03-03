namespace Project.Models
{
    public class InvoiceModel
    {
        public int InvoiceId { get; set; }
        public int ProformaInvoiceId { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        
        public string Descriptions { get; set; }
        public string UnitPrices { get; set; }
        public string Qtys { get; set; }
        public string SubTotals { get; set; }
        public string TaxAmounts { get; set; }
        public string Totals { get; set; }
        
        public double SubTotal { get; set; }
        public double TotalTaxAmount { get; set; }
        public double GrandTotal { get; set; }
        public string CreatedBy { get; set; }
        public string Remarks { get; set; }
        
        //
        public ProformaInvoiceItem[] Items { get; set; }
        public ProformaInvoiceModel ProformaInvoice { get; set; }
        public string AmountString { get; set; }
    }

    
}
