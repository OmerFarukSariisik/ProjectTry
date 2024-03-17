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
        var taxDocumentationsModel = new TaxDocumentationsModel
        {
            AllMonthsTaxAmounts = allMonthTaxAmount
        };
        return View(taxDocumentationsModel);
    }
}