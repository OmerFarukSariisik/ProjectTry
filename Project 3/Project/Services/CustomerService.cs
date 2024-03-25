using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Services;

public enum DocumentType
{
    TranslationForm = 1,
    AttestationForm = 2,
    PoaDubai = 3,
    PoaAbuDhabi = 4,
    SignatureSirculary = 5,
    Invitation = 6,
    CommitmentLetter = 7,
    ConsentLetter = 8
}

public interface ICustomerService
{
    List<Customer> AllCustomers { get; set; }

    void Initialize();
    void CreateCustomer(Customer customer);
}

public class CustomerService : ICustomerService
{
    public List<Customer> AllCustomers { get; set; } = new();

    private readonly string _connectionString;
    private bool _isInitialized;

    public CustomerService(IConfiguration configuration)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'customers') CREATE TABLE customers " +
                "(CustomerId INT PRIMARY KEY IDENTITY(1,1), FullName NVARCHAR(50) NOT NULL, MotherName NVARCHAR(50), Address NVARCHAR(MAX), Email NVARCHAR(50), MobileNumber NVARCHAR(20)" +
                ", WhatsAppNumber NVARCHAR(20), TRNNumber NVARCHAR(20), Satisfaction NVARCHAR(3))";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }

            string sql = "SELECT * FROM customers";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customer customer = new Customer();
                        customer.CustomerId = reader.GetInt32(0);
                        Console.WriteLine(
                            "Getting customer with id: " + customer.CustomerId + " - " + customer.FullName);
                        if (AllCustomers.Any(c => c.CustomerId == customer.CustomerId))
                            continue;
                        Console.WriteLine("Got customer with id: " + customer.CustomerId + " - " + customer.FullName);
                        customer.FullName = reader.GetString(1);
                        customer.MotherName = reader.GetString(2);
                        customer.Address = reader.GetString(3);
                        customer.Email = reader.GetString(4);
                        customer.MobileNumber = reader.GetString(5);
                        customer.WhatsAppNumber = reader.GetString(6);
                        customer.TRNNumber = reader.GetString(7);
                        customer.Satisfaction = reader.GetString(8);
                        
                        Console.WriteLine("Mobile number:" + customer.MobileNumber + "-");
                        if (AllCustomers.All(c => c.CustomerId != customer.CustomerId))
                            AllCustomers.Add(customer);
                    }
                }
            }
        }

        _isInitialized = true;
    }

    public void CreateCustomer(Customer customer)
    {
        Initialize();
        Customer newCustomer = new Customer
        {
            FullName = customer.FullName,
            MotherName = string.IsNullOrEmpty(customer.MotherName) ? "" : customer.MotherName,
            Address = customer.Address,
            Email = customer.Email,
            MobileNumber = customer.MobileNumber,
            WhatsAppNumber = customer.WhatsAppNumber,
            TRNNumber = customer.TRNNumber,
            Satisfaction = customer.Satisfaction
        };

        var hasCustomer = AllCustomers.Any(c => c.FullName == newCustomer.FullName && c.Email == newCustomer.Email);
        if (hasCustomer)
            return;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string sql = "INSERT INTO customers (FullName, MotherName, Address, Email, MobileNumber, WhatsAppNumber, TRNNumber, Satisfaction) " +
                         "VALUES (@FullName, @MotherName, @Address, @Email, @MobileNumber, @WhatsAppNumber, @TRNNumber, @Satisfaction) SELECT SCOPE_IDENTITY()";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@FullName", newCustomer.FullName);
                command.Parameters.AddWithValue("@MotherName", newCustomer.MotherName);
                command.Parameters.AddWithValue("@Address", newCustomer.Address);
                command.Parameters.AddWithValue("@Email", newCustomer.Email);
                command.Parameters.AddWithValue("@MobileNumber", newCustomer.MobileNumber);
                command.Parameters.AddWithValue("@WhatsAppNumber", newCustomer.WhatsAppNumber);
                command.Parameters.AddWithValue("@TRNNumber", newCustomer.TRNNumber);
                command.Parameters.AddWithValue("@Satisfaction", newCustomer.Satisfaction);
                Console.WriteLine("Execute");
                newCustomer.CustomerId = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        Console.WriteLine("Added customer with id: " + newCustomer.CustomerId + " - " + newCustomer.FullName);
        AllCustomers.Add(newCustomer);
    }
}