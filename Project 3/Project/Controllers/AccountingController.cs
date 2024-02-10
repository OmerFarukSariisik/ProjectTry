using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System.Data.SqlClient;

namespace Project.Controllers
{
    public class AccountingController : Controller
    {
        public List<Bill> AllBills { get; set; } = new();
        private readonly string _connectionString;
        
        private bool _isInitialized;

        public AccountingController(IConfiguration configuration) 
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string tableExistsSql =
                    "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'bills') CREATE TABLE bills " +
                    "(InvoiceId INT PRIMARY KEY IDENTITY(1,1), InvoiceNumber NVARCHAR(20) NOT NULL, InvoiceDate DATE, Currency NVARCHAR(10), CustomerFullName NVARCHAR(30)" +
                    ", ContactNumber NVARCHAR(20), Date DATE, DeliveryDate DATE, Signature VARBINARY(MAX))";

                using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
                {
                    checkTableCommand.ExecuteNonQuery();
                }

                string sql = "SELECT * FROM bills";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Bill bill = new Bill();
                            bill.InvoiceId = reader.GetInt32(0);
                            bill.InvoiceNumber = reader.GetString(1);
                            bill.InvoiceDate = reader.GetDateTime(2);
                            bill.Currency = reader.GetString(3);
                            bill.CustomerFullName = reader.GetString(4);
                            bill.ContactNumber = reader.GetString(5);
                            bill.Date = reader.GetDateTime(6);
                            bill.DeliveryDate = reader.GetDateTime(7);

                            int columnIndex = reader.GetOrdinal("Signature");
                            long byteLength = reader.GetBytes(columnIndex, 0, null, 0, 0);
                            byte[] signatureData = new byte[byteLength];
                            reader.GetBytes(columnIndex, 0, signatureData, 0, (int)byteLength);
                            bill.Signature = signatureData;
                            bill.SignatureString = "data:image/jpeg;base64," + Convert.ToBase64String(bill.Signature);

                            AllBills.Add(bill);
                        }
                    }
                }
            }
            
            _isInitialized = true;
        }

        public IActionResult Index(string str, bool first)
        {
            Initialize();
            if(string.IsNullOrEmpty(str))
                return View(AllBills.ToArray());
            
            var filteredBills = new List<Bill>();
            foreach (var bill in AllBills)
            {
                if (first && bill.InvoiceNumber.Contains(str))
                    filteredBills.Add(bill);
                if(!first && bill.CustomerFullName.Contains(str))
                    filteredBills.Add(bill);
            }

            return View(filteredBills.ToArray());
        }

        public IActionResult CreatePage()
        {
            return View();
        }

        public IActionResult EditPage(int targetInvoiceId)
        {
            Initialize();
            var retrievedBill = AllBills.Find(b => b.InvoiceId == targetInvoiceId);
            return View(retrievedBill);
        }

        [HttpPost]
        public IActionResult Create(Bill bill)
        {
            Bill newBill = new Bill
            {
                InvoiceNumber = bill.InvoiceNumber,
                InvoiceDate = bill.InvoiceDate,
                Currency = bill.Currency,
                CustomerFullName = bill.CustomerFullName,
                ContactNumber = bill.ContactNumber,
                Date = bill.Date,
                DeliveryDate = bill.DeliveryDate,
                Signature = bill.Signature,
                SignatureString = bill.SignatureString
            };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO bills (InvoiceNumber, InvoiceDate, Currency, CustomerFullName, ContactNumber, Date, DeliveryDate, Signature) " + 
                    "VALUES (@InvoiceNumber, @InvoiceDate, @Currency, @CustomerFullName, @ContactNumber, @Date, @DeliveryDate, @Signature)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceNumber", newBill.InvoiceNumber);
                    command.Parameters.AddWithValue("@InvoiceDate", newBill.InvoiceDate);
                    command.Parameters.AddWithValue("@Currency", newBill.Currency);
                    command.Parameters.AddWithValue("@CustomerFullName", newBill.CustomerFullName);
                    command.Parameters.AddWithValue("@ContactNumber", newBill.ContactNumber);
                    command.Parameters.AddWithValue("@Date", newBill.Date);
                    command.Parameters.AddWithValue("@DeliveryDate", newBill.DeliveryDate);
                    command.Parameters.AddWithValue("@Signature", Convert.FromBase64String(newBill.SignatureString.Split(',')[1]));
                    command.ExecuteNonQuery();
                }
            }
            
            AllBills.Add(newBill);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(Bill bill)
        {
            Bill newBill = new Bill
            {
                InvoiceId = bill.InvoiceId,
                InvoiceNumber = bill.InvoiceNumber,
                InvoiceDate = bill.InvoiceDate,
                Currency = bill.Currency,
                CustomerFullName = bill.CustomerFullName,
                ContactNumber = bill.ContactNumber,
                Date = bill.Date,
                DeliveryDate = bill.DeliveryDate,
                SignatureString = bill.SignatureString
            };
            
            var signatureToSave = newBill.SignatureString.Split(',')[1];
            newBill.Signature = Convert.FromBase64String(signatureToSave);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "UPDATE bills SET InvoiceNumber = @InvoiceNumber, " +
                "InvoiceDate = @InvoiceDate, " +
                "Currency = @Currency, " +
                "CustomerFullName = @CustomerFullName, " +
                "ContactNumber = @ContactNumber, " +
                "Date = @Date, " +
                "DeliveryDate = @DeliveryDate, " +
                "Signature = @Signature " +
                "WHERE InvoiceId = @InvoiceId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceId", newBill.InvoiceId);
                    command.Parameters.AddWithValue("@InvoiceNumber", newBill.InvoiceNumber);
                    command.Parameters.AddWithValue("@InvoiceDate", newBill.InvoiceDate);
                    command.Parameters.AddWithValue("@Currency", newBill.Currency);
                    command.Parameters.AddWithValue("@CustomerFullName", newBill.CustomerFullName);
                    command.Parameters.AddWithValue("@ContactNumber", newBill.ContactNumber);
                    command.Parameters.AddWithValue("@Date", newBill.Date);
                    command.Parameters.AddWithValue("@DeliveryDate", newBill.DeliveryDate);
                    command.Parameters.AddWithValue("@Signature", newBill.Signature);
                    command.ExecuteNonQuery();
                }
            }

            Initialize();
            var retrievedBill = AllBills.Find(b => b.InvoiceId == newBill.InvoiceId);
            var index = AllBills.IndexOf(retrievedBill);
            AllBills[index] = newBill;
            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM bills WHERE InvoiceId = @InvoiceId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceId", id);

                    command.ExecuteNonQuery();
                }
            }
            
            Initialize();
            AllBills.RemoveAll(b => b.InvoiceId == id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Search(string searchString)
        {
            /*Initialize();
            Console.WriteLine($"SearchString: {searchString} + {AllBills.Count}");
            var filteredBills = AllBills.FindAll(b => b.InvoiceNumber.Contains(searchString));
            Console.WriteLine($"FilteredBills: {filteredBills.Count}");*/
            return RedirectToAction("Index", new { str = searchString, first = true });
        }
        [HttpPost]
        public IActionResult SearchByName(string searchNameString)
        {
            /*Initialize();
            Console.WriteLine($"SearchString2: {searchNameString} + {AllBills.Count}");
            var filteredBills = AllBills.FindAll(b => b.CustomerFullName.Contains(searchNameString));
            Console.WriteLine($"FilteredBills: {filteredBills.Count}");*/
            return RedirectToAction("Index", new { str = searchNameString, first = false });
        }

        public IActionResult ShowInvoice(int id)
        {
            Initialize();
            var retrievedBill = AllBills.Find(b => b.InvoiceId == id);
            return View(retrievedBill);
        }
    }
}
