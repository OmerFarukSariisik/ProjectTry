using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project.Controllers;
using Project.Data;
using Project.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProformaInvoiceService, ProformaInvoiceService>();
builder.Services.AddScoped<ITaxManagementService, TaxManagementService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IDocumentAttestationService, DocumentAttestationService>();
builder.Services.AddScoped<IDocumentTranslationService, DocumentTranslationService>();
builder.Services.AddScoped<IDocumentPoaService, DocumentPoaService>();
builder.Services.AddScoped<IDocumentCommitmentLetterService, DocumentCommitmentLetterService>();
builder.Services.AddScoped<IDocumentConsentLetterService, DocumentConsentLetterService>();
builder.Services.AddScoped<IDocumentInvitationService, DocumentInvitationService>();
builder.Services.AddScoped<IDocumentSircularyService, DocumentSircularyService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{proformaInvoiceId?}");
app.MapRazorPages();

app.Run();
