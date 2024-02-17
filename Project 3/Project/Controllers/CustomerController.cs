using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System.Data.SqlClient;
using Project.Services;

namespace Project.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IDocumentTranslationService _documentTranslationService;
        
        private readonly string _connectionString;

        public CustomerController(IConfiguration configuration, IDocumentTranslationService documentTranslationService)
        {
            _documentTranslationService = documentTranslationService;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult CustomerIndex(string searchString)
        {
            _documentTranslationService.Initialize();
            if (string.IsNullOrEmpty(searchString))
                return View(_documentTranslationService.AllCustomers.ToArray());
            
            return View(_documentTranslationService.AllCustomers.FindAll(c => c.FullName.Contains(searchString)).ToArray());
        }

        public IActionResult CreateCustomerPage()
        {
            return View();
        }

        public IActionResult EditCustomerPage(int id)
        {
            _documentTranslationService.Initialize();
            var retrievedCustomer =
                _documentTranslationService.AllCustomers.FirstOrDefault(customer => customer.CustomerId == id);

            return View(retrievedCustomer);
        }

        [HttpPost]
        public IActionResult CreateCustomerAndRedirect(Customer customer)
        {
            //CreateCustomer(customer);
            return RedirectToAction("CustomerIndex");
        }

        [HttpPost]
        public void CreateCustomer(Customer customer)
        {
            /*Initialize();
            Customer newCustomer = new Customer
            {
                FullName = customer.FullName,
                Address = customer.Address,
                Email = customer.Email,
                MobileNumber = customer.MobileNumber,
                TRNNumber = customer.TRNNumber
            };
            
            var hasCustomer = _documentTranslationService.AllCustomers.Any(c => c.FullName == newCustomer.FullName && c.Email == newCustomer.Email);
            if (hasCustomer)
                return;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO customers (FullName, Address, Email, MobileNumber, TRNNumber) " +
                    "VALUES (@FullName, @Address, @Email, @MobileNumber, @TRNNumber); SELECT SCOPE_IDENTITY()";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@FullName", newCustomer.FullName);
                    command.Parameters.AddWithValue("@Address", newCustomer.Address);
                    command.Parameters.AddWithValue("@Email", newCustomer.Email);
                    command.Parameters.AddWithValue("@MobileNumber", newCustomer.MobileNumber);
                    command.Parameters.AddWithValue("@TRNNumber", newCustomer.TRNNumber);
                    command.ExecuteNonQuery();
                    newCustomer.CustomerId = (int)command.ExecuteScalar();
                }
            }
            
            _documentTranslationService.AllCustomers.Add(newCustomer);
            Console.WriteLine("Last Customer(Customer): " + _documentTranslationService.AllCustomers.Last().FullName + " - " +
                              _documentTranslationService.AllCustomers.Last().CustomerId);*/
        }

        [HttpPost]
        public IActionResult Edit(Customer customer)
        {
            Customer newCustomer = new Customer
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                MotherName = string.IsNullOrEmpty(customer.MotherName) ? "" : customer.MotherName,
                Address = customer.Address,
                Email = customer.Email,
                MobileNumber = customer.MobileNumber,
                WhatsAppNumber = customer.WhatsAppNumber,
                TRNNumber = string.IsNullOrEmpty(customer.TRNNumber) ? "" : customer.TRNNumber,
                Satisfaction = customer.Satisfaction
            };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "UPDATE customers SET FullName = @FullName, " +
                "MotherName = @MotherName, " +
                "Address = @Address, " +
                "Email = @Email, " +
                "MobileNumber = @MobileNumber, " +
                "WhatsAppNumber = @WhatsAppNumber, " +
                "TRNNumber = @TRNNumber, " +
                "Satisfaction = @Satisfaction " +
                "WHERE CustomerId = @CustomerId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", newCustomer.CustomerId);
                    command.Parameters.AddWithValue("@FullName", newCustomer.FullName);
                    command.Parameters.AddWithValue("@MotherName", newCustomer.MotherName);
                    command.Parameters.AddWithValue("@Address", newCustomer.Address);
                    command.Parameters.AddWithValue("@Email", newCustomer.Email);
                    command.Parameters.AddWithValue("@MobileNumber", newCustomer.MobileNumber);
                    command.Parameters.AddWithValue("@WhatsAppNumber", newCustomer.WhatsAppNumber);
                    command.Parameters.AddWithValue("@TRNNumber", newCustomer.TRNNumber);
                    command.Parameters.AddWithValue("@Satisfaction", newCustomer.Satisfaction);
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("CustomerIndex");
        }


        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM customers WHERE CustomerId = @CustomerId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", id);
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("CustomerIndex");
        }
    }
}
