using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System.Data.SqlClient;
using Project.Services;

namespace Project.Controllers
{
    public class VoucherController : Controller
    {
        private readonly IProformaInvoiceService _proformaInvoiceService;
        private readonly IDocumentTranslationService _documentTranslationService;
        public List<Voucher> AllVouchers { get; set; } = new();
        private readonly string _connectionString;
        private bool _isInitialized;

        public VoucherController(IConfiguration configuration, IDocumentTranslationService documentTranslationService,
            IProformaInvoiceService proformaInvoiceService)
        {
            _documentTranslationService = documentTranslationService;
            _proformaInvoiceService = proformaInvoiceService;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        public void Initialize()
        {
            if (_isInitialized)
                return;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                string tableExistsSql = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'vouchers') CREATE TABLE vouchers " +
                                        "(VoucherId INT PRIMARY KEY IDENTITY(1,1), ProformaInvoiceId INT NOT NULL, Amount FLOAT, Date DATE)";

                using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
                {
                    checkTableCommand.ExecuteNonQuery();
                }

                string sql = "SELECT * FROM vouchers";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var voucherModel = new Voucher();
                            voucherModel.VoucherId = reader.GetInt32(0);
                            voucherModel.ProformaInvoiceId = reader.GetInt32(1);
                            voucherModel.Amount = reader.GetDouble(2);
                            voucherModel.Date = reader.GetDateTime(3);

                            _proformaInvoiceService.Initialize();
                            var proformaInvoice = _proformaInvoiceService.AllProformaInvoices.Find(c =>
                                c.ProformaInvoiceId == voucherModel.ProformaInvoiceId);
                            _documentTranslationService.Initialize();
                            var customer = _documentTranslationService.AllCustomers.Find(c =>
                                c.CustomerId == proformaInvoice.CustomerId);

                            if (customer == null)
                            {
                                Console.WriteLine("customer is null for voucher:" + voucherModel.ProformaInvoiceId);
                                continue;
                            }

                            voucherModel.Customer = customer;
                            AllVouchers.Add(voucherModel);
                        }
                    }
                }
            }

            _isInitialized = true;
        }

        public IActionResult VoucherIndex()
        {
            Initialize();
            return View(AllVouchers.ToArray());
        }

        public IActionResult CreateVoucherPage(ProformaInvoiceModel proformaInvoiceModel)
        {
            _documentTranslationService.Initialize();
            proformaInvoiceModel.Customer = _documentTranslationService.AllCustomers.Find(c =>
                c.CustomerId == proformaInvoiceModel.CustomerId);
            var voucherModel = new Voucher
            {
                ProformaInvoiceId = proformaInvoiceModel.ProformaInvoiceId,
                Customer = proformaInvoiceModel.Customer
            };
            return View(voucherModel);
        }

        public IActionResult EditVoucherPage(int id)
        {
            Initialize();
            var retrievedVoucher = AllVouchers.Find(b => b.VoucherId == id);
            return View(retrievedVoucher);
        }


        [HttpPost]
        public IActionResult Create(Voucher voucher)
        {
            Voucher newVoucher = new Voucher
            {
                ProformaInvoiceId = voucher.ProformaInvoiceId,
                Amount = voucher.Amount,
                Date = voucher.Date
            };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO vouchers (ProformaInvoiceId, Amount, Date) " +
                    "VALUES (@ProformaInvoiceId, @Amount, @Date)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ProformaInvoiceId", newVoucher.ProformaInvoiceId);
                    command.Parameters.AddWithValue("@Amount", newVoucher.Amount);
                    command.Parameters.AddWithValue("@Date", newVoucher.Date);

                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("VoucherIndex");
        }


        [HttpPost]
        public IActionResult Edit(Voucher voucher)
        {
            Voucher newVoucher = new Voucher
            {
                VoucherId = voucher.VoucherId,
                ProformaInvoiceId = voucher.ProformaInvoiceId,
                Amount = voucher.Amount,
                Date = voucher.Date
            };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "UPDATE vouchers SET ProformaInvoiceId = @ProformaInvoiceId, " +
                "Amount = @Amount, " +
                "Date = @Date " +
                "WHERE VoucherId = @VoucherId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@VoucherId", newVoucher.VoucherId);
                    command.Parameters.AddWithValue("@ProformaInvoiceId", newVoucher.ProformaInvoiceId);
                    command.Parameters.AddWithValue("@Amount", newVoucher.Amount);
                    command.Parameters.AddWithValue("@Date", newVoucher.Date);

                    command.ExecuteNonQuery();
                }
            }

            Initialize();
            var retrievedPending =
                AllVouchers.Find(b => b.VoucherId == newVoucher.VoucherId);
            var index = AllVouchers.IndexOf(retrievedPending);
            AllVouchers[index] = newVoucher;
            return RedirectToAction("VoucherIndex");
        }

        public IActionResult DeleteVoucher(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM vouchers WHERE VoucherId = @VoucherId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@VoucherId", id);

                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("VoucherIndex");
        }
        
        public IActionResult ShowVoucherPage(int id)
        {
            Initialize();
            var retrievedVoucher = AllVouchers.Find(b => b.VoucherId == id);
            _proformaInvoiceService.Initialize();
            var retrievedProformaInvoice =
                _proformaInvoiceService.AllProformaInvoices.Find(b =>
                    b.ProformaInvoiceId == retrievedVoucher.ProformaInvoiceId);
            _documentTranslationService.Initialize();
            retrievedVoucher.Customer = _documentTranslationService.AllCustomers.Find(c =>
                c.CustomerId == retrievedProformaInvoice.CustomerId);
            return View(retrievedVoucher);
        }
    }
}
