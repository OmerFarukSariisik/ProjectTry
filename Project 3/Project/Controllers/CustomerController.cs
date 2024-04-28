using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using Project.Services;

namespace Project.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ISettingsService _settingsService;

        private readonly string _connectionString;

        public CustomerController(IConfiguration configuration, ICustomerService customerService,
            ISettingsService settingsService)
        {
            _customerService = customerService;
            _settingsService = settingsService;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult CustomerIndex(string searchString)
        {
            _customerService.Initialize();
            if (string.IsNullOrEmpty(searchString))
                return View(_customerService.AllCustomers.ToArray());

            return View(_customerService.AllCustomers.FindAll(c => c.FullName.Contains(searchString)).ToArray());
        }

        public IActionResult CreateCustomerPage()
        {
            return View();
        }

        public IActionResult EditCustomerPage(int id)
        {
            _customerService.Initialize();
            var retrievedCustomer =
                _customerService.AllCustomers.FirstOrDefault(customer => customer.CustomerId == id);

            return View(retrievedCustomer);
        }

        public IActionResult MailToCustomerPage(int id)
        {
            _settingsService.Initialize();
            _customerService.Initialize();
            var retrievedCustomer = _customerService.AllCustomers.FirstOrDefault(customer => customer.CustomerId == id);

            var mailModel = new MailModel
            {
                To = retrievedCustomer.Email,
                From = _settingsService.AllSettings.First().Mail,
                Password = _settingsService.AllSettings.First().Password
            };
            return View(mailModel);
        }

        [HttpPost]
        public IActionResult SendMail(MailModel mailModel)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(mailModel.From, mailModel.Password),
                    EnableSsl = true,
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(mailModel.From),
                    Subject = mailModel.Subject,
                    Body = mailModel.Body,
                    IsBodyHtml = true,
                };
                var toEmails = mailModel.To.Split(',');
                
                foreach (var email in toEmails)
                {
                    mailMessage.To.Add(email.Trim()); // Trim to remove any leading or trailing spaces
                }

                smtpClient.Send(mailMessage);

                Console.WriteLine("MAIL SENT");
            }
            catch (Exception ex)
            {
                Console.WriteLine("MAIL NOT SENT");
            }
            
            return RedirectToAction("CustomerIndex");
        }
        
        public ActionResult SendMailToAllCustomers()
        {
            _customerService.Initialize();
            var allCustomerEmails = string.Join(",", _customerService.AllCustomers.Select(c => c.Email));
            
            var mailModel = new MailModel
            {
                To = allCustomerEmails
            };

            // Redirect to SendMail view with mailModel
            return View("MailToCustomerPage", mailModel);
        }

        [HttpPost]
        public IActionResult CreateCustomerAndRedirect(Customer customer)
        {
            //CreateCustomer(customer);
            return RedirectToAction("CustomerIndex");
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