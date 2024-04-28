using System.Data.SqlClient;
using System.Globalization;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Services;

public interface ISettingsService
{
    List<SettingsModel> AllSettings { get; set; }

    void Initialize();
    void EditSettings(SettingsModel settingsModel);
}

public class SettingsService : ISettingsService
{
    public List<SettingsModel> AllSettings { get; set; } = new();

    private readonly string _connectionString;
    private bool _isInitialized;

    public SettingsService(IConfiguration configuration)
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
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'settings') CREATE TABLE settings " +
                "(SettingsId INT PRIMARY KEY IDENTITY(1,1), AttestationTax INT, CommitmentTax INT, ConsentTax INT" +
                ", InvitationTax INT, PoaTax INT, SircularyTax INT, TranslationTax INT, Mail NVARCHAR(30)" +
                ", Password NVARCHAR(30), AttestationPrice FLOAT, CommitmentPrice FLOAT, ConsentPrice FLOAT" +
                ", InvitationPrice FLOAT, PoaPrice FLOAT, SircularyPrice FLOAT, TranslationPrice FLOAT)";

            using (SqlCommand checkTableCommand = new SqlCommand(tableExistsSql, connection))
            {
                checkTableCommand.ExecuteNonQuery();
            }

            string sql = "SELECT * FROM settings";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var settingsModel = new SettingsModel();
                        settingsModel.SettingsId = reader.GetInt32(0);
                        settingsModel.AttestationTax = reader.GetInt32(1);
                        settingsModel.CommitmentTax = reader.GetInt32(2);
                        settingsModel.ConsentTax = reader.GetInt32(3);
                        settingsModel.InvitationTax = reader.GetInt32(4);
                        settingsModel.PoaTax = reader.GetInt32(5);
                        settingsModel.SircularyTax = reader.GetInt32(6);
                        settingsModel.TranslationTax = reader.GetInt32(7);
                        settingsModel.Mail = reader.GetString(8);
                        settingsModel.Password = reader.GetString(9);
                        settingsModel.AttestationPrice = reader.GetDouble(10);
                        settingsModel.CommitmentPrice = reader.GetDouble(11);
                        settingsModel.ConsentPrice = reader.GetDouble(12);
                        settingsModel.InvitationPrice = reader.GetDouble(13);
                        settingsModel.PoaPrice = reader.GetDouble(14);
                        settingsModel.SircularyPrice = reader.GetDouble(15);
                        settingsModel.TranslationPrice = reader.GetDouble(16);

                        AllSettings.Add(settingsModel);
                    }
                }
            }
        }

        if (AllSettings.Count == 0)
        {
            var settingsModel = new SettingsModel
            {
                SettingsId = 1,
                AttestationTax = 0,
                CommitmentTax = 0,
                ConsentTax = 0,
                InvitationTax = 0,
                PoaTax = 0,
                SircularyTax = 0,
                TranslationTax = 0,
                Mail = "",
                Password = "",
                AttestationPrice = 0,
                CommitmentPrice = 0,
                ConsentPrice = 0,
                InvitationPrice = 0,
                PoaPrice = 0,
                SircularyPrice = 0,
                TranslationPrice = 0
            };
            AllSettings.Add(settingsModel);


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql =
                    "INSERT INTO settings (AttestationTax, CommitmentTax, ConsentTax, InvitationTax, PoaTax, SircularyTax, TranslationTax, Mail, Password, AttestationPrice, CommitmentPrice, ConsentPrice, InvitationPrice, PoaPrice, SircularyPrice, TranslationPrice) " +
                    "VALUES (@AttestationTax, @CommitmentTax, @ConsentTax, @InvitationTax, @PoaTax, @SircularyTax, @TranslationTax, @Mail, @Password, @AttestationPrice, @CommitmentPrice, @ConsentPrice, @InvitationPrice, @PoaPrice, @SircularyPrice, @TranslationPrice)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@AttestationTax", settingsModel.AttestationTax);
                    command.Parameters.AddWithValue("@CommitmentTax", settingsModel.CommitmentTax);
                    command.Parameters.AddWithValue("@ConsentTax", settingsModel.ConsentTax);
                    command.Parameters.AddWithValue("@InvitationTax", settingsModel.InvitationTax);
                    command.Parameters.AddWithValue("@PoaTax", settingsModel.PoaTax);
                    command.Parameters.AddWithValue("@SircularyTax", settingsModel.SircularyTax);
                    command.Parameters.AddWithValue("@TranslationTax", settingsModel.TranslationTax);
                    command.Parameters.AddWithValue("@Mail", settingsModel.Mail);
                    command.Parameters.AddWithValue("@Password", settingsModel.Password);
                    command.Parameters.AddWithValue("@AttestationPrice", settingsModel.AttestationPrice);
                    command.Parameters.AddWithValue("@CommitmentPrice", settingsModel.CommitmentPrice);
                    command.Parameters.AddWithValue("@ConsentPrice", settingsModel.ConsentPrice);
                    command.Parameters.AddWithValue("@InvitationPrice", settingsModel.InvitationPrice);
                    command.Parameters.AddWithValue("@PoaPrice", settingsModel.PoaPrice);
                    command.Parameters.AddWithValue("@SircularyPrice", settingsModel.SircularyPrice);
                    command.Parameters.AddWithValue("@TranslationPrice", settingsModel.TranslationPrice);
                    command.ExecuteNonQuery();
                }
            }
        }

        _isInitialized = true;
    }

    [HttpPost]
    public void EditSettings(SettingsModel settingsModel)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = "UPDATE settings SET AttestationTax = @AttestationTax, " +
                         "CommitmentTax = @CommitmentTax, " +
                         "ConsentTax = @ConsentTax, " +
                         "InvitationTax = @InvitationTax, " +
                         "PoaTax = @PoaTax, " +
                         "SircularyTax = @SircularyTax, " +
                         "TranslationTax = @TranslationTax, " +
                         "Mail = @Mail, " +
                         "Password = @Password, " +
                         "AttestationPrice = @AttestationPrice, " +
                         "CommitmentPrice = @CommitmentPrice, " +
                         "ConsentPrice = @ConsentPrice, " +
                         "InvitationPrice = @InvitationPrice, " +
                         "PoaPrice = @PoaPrice, " +
                         "SircularyPrice = @SircularyPrice, " +
                         "TranslationPrice = @TranslationPrice " +
                         "WHERE SettingsId = @SettingsId";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SettingsId", settingsModel.SettingsId);
                command.Parameters.AddWithValue("@AttestationTax", settingsModel.AttestationTax);
                command.Parameters.AddWithValue("@CommitmentTax", settingsModel.CommitmentTax);
                command.Parameters.AddWithValue("@ConsentTax", settingsModel.ConsentTax);
                command.Parameters.AddWithValue("@InvitationTax", settingsModel.InvitationTax);
                command.Parameters.AddWithValue("@PoaTax", settingsModel.PoaTax);
                command.Parameters.AddWithValue("@SircularyTax", settingsModel.SircularyTax);
                command.Parameters.AddWithValue("@TranslationTax", settingsModel.TranslationTax);
                command.Parameters.AddWithValue("@Mail", settingsModel.Mail);
                command.Parameters.AddWithValue("@Password", settingsModel.Password);
                command.Parameters.AddWithValue("@AttestationPrice", settingsModel.AttestationPrice);
                command.Parameters.AddWithValue("@CommitmentPrice", settingsModel.CommitmentPrice);
                command.Parameters.AddWithValue("@ConsentPrice", settingsModel.ConsentPrice);
                command.Parameters.AddWithValue("@InvitationPrice", settingsModel.InvitationPrice);
                command.Parameters.AddWithValue("@PoaPrice", settingsModel.PoaPrice);
                command.Parameters.AddWithValue("@SircularyPrice", settingsModel.SircularyPrice);
                command.Parameters.AddWithValue("@TranslationPrice", settingsModel.TranslationPrice);
                command.ExecuteNonQuery();
            }
        }
    }
}