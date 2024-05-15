using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class TaxDocumentationsController : Controller
{
    private IInvoiceService _invoiceService;
    private ITaxManagementService _taxManagementService;
    
    public TaxDocumentationsController(IInvoiceService invoiceService, ITaxManagementService taxManagementService)
    {
        _invoiceService = invoiceService;
        _taxManagementService = taxManagementService;
    }
    
    public IActionResult TaxDocumentationsPage()
    {
        var allMonthTaxAmount = _taxManagementService.GetAllMonthsTaxAmounts();
        var allYearTaxAmount = _taxManagementService.GetAllYearsTaxAmounts();
        var allDayTaxAmount = _taxManagementService.GetAllDaysTaxAmounts();
        var allDayTaxData = _taxManagementService.GetAllDayTaxData();
        var taxDocumentationsModel = new TaxDocumentationsModel
        {
            AllMonthsTaxAmounts = allMonthTaxAmount,
            AllYearsTaxAmounts = allYearTaxAmount,
            AllDaysTaxAmounts = allDayTaxAmount,
            MonthTaxAmountsString = string.Join("W",
                allMonthTaxAmount.Select(kv =>
                    $"{kv.Key.year}-{kv.Key.month}Q{kv.Value.subTotal}Q{kv.Value.totalTaxAmount}Q{kv.Value.grandTotal}")),
            YearTaxAmountsString = string.Join("W",  
                allYearTaxAmount.Select(kv =>
                    $"{kv.Key}Q{kv.Value.subTotal}Q{kv.Value.totalTaxAmount}Q{kv.Value.grandTotal}")),
            DayTaxAmountsString = string.Join("W",  
                allDayTaxAmount.Select(kv =>
                    $"{kv.Key}Q{kv.Value.subTotal}Q{kv.Value.totalTaxAmount}Q{kv.Value.grandTotal}")),
            DayTaxData = allDayTaxData
        };
        return View(taxDocumentationsModel);
    }
}