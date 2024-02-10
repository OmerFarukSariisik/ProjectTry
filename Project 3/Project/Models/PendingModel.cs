namespace Project.Models;

public class PendingModel
{
    public int PendingId { get; set; }
    public int ProformaInvoiceId { get; set; }
    public string Note { get; set; }

    public ProformaInvoiceModel ProformaInvoice { get; set; }
}