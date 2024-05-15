using System.Data.SqlClient;
using Project.Models;

namespace Project.Services
{
    public interface ILoginService
    {
        List<LoginModel> AllLoginModels { get; set; }
        void Initialize();
    }

    public class LoginService : ILoginService
    {
        public List<LoginModel> AllLoginModels { get; set; } = new();
        private readonly string _connectionString;
        private bool _isInitialized;

        public LoginService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Add initialization SQL commands if needed
                string tableExistsSql =
                    "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'users') " +
                    "CREATE TABLE users (UserId INT PRIMARY KEY IDENTITY(1,1), Username NVARCHAR(50), Password NVARCHAR(50))";

                using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
                {
                    checkTableCommand.ExecuteNonQuery();
                }
                
                string sql = "SELECT * FROM users";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LoginModel loginModel = new LoginModel();
                            loginModel.Username = reader.GetString(0);
                            loginModel.Password = reader.GetString(1);
                            AllLoginModels.Add(loginModel);
                        }
                    }
                }
            }
            
            var adminLoginModel = new LoginModel
            {
                Username = "admin",
                Password = "112233"
            };
            AllLoginModels.Add(adminLoginModel);

            _isInitialized = true;
        }
    }
}