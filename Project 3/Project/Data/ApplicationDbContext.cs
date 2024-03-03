using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<DocumentAttestationModel> DocumentAttestationModel { get; set; }
        public DbSet<DocumentCommitmentLetterModel> DocumentCommitmentLetterModel { get; set; }
        public DbSet<DocumentConsentLetterModel> DocumentConsentLetterModel { get; set; }
        public DbSet<DocumentInvitationModel> DocumentInvitationModel { get; set; }
        public DbSet<DocumentPoaModel> DocumentPoaModel { get; set; }
        public DbSet<DocumentSircularyModel> DocumentSircularyModel { get; set; }
        public DbSet<DocumentTranslationModel> DocumentTranslationModel { get; set; }
        public DbSet<InvoiceModel> InvoiceModel { get; set; }
        public DbSet<PendingModel> PendingModel { get; set; }
        public DbSet<ProformaInvoiceModel> ProformaInvoiceModel { get; set; }
        public DbSet<Voucher> Voucher { get; set; }
    }
}