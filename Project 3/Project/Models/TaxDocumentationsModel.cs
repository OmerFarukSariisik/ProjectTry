namespace Project.Models;

public class TaxDocumentationsModel
{
    public Dictionary<(int year, int month), (double subTotal, double totalTaxAmount, double grandTotal)> AllMonthsTaxAmounts { get; set; }
    public Dictionary<int, (double subTotal, double totalTaxAmount, double grandTotal)> AllYearsTaxAmounts { get; set; }
    public Dictionary<string, (double subTotal, double totalTaxAmount, double grandTotal)> AllDaysTaxAmounts { get; set; }

    public string MonthTaxAmountsString { get; set; }
    public string YearTaxAmountsString { get; set; }
    public string DayTaxAmountsString { get; set; }
    public List<DayTaxData> DayTaxData { get; set; }
}

public class DayTaxData
{
    public DateTime Date { get; set; }
    public double SubTotal { get; set; }
    public double TotalTaxAmount { get; set; }
    public double GrandTotal { get; set; }
}