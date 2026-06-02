using Microsoft.Data.SqlClient;
using SchoolManagementSystem.Security;
using SchoolManagementSystem.UI;
using System.Windows;

namespace SchoolManagementSystem
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CreateAdminUserOnce();

            new LoginWindow().Show();
        }

        private void CreateAdminUserOnce()
        {
            PasswordHelper.CreatePasswordHash("1234", out byte[] hash, out byte[] salt);

            using var conn = new SqlConnection(
                "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=SchoolDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
            );

            conn.Open();

            string sql = @"
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
                BEGIN
                    INSERT INTO Users (Username, PasswordHash, PasswordSalt, Role, IsActive)
                    VALUES (@Username, @Hash, @Salt, 'Admin', 1)
                END";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@Username", "admin");
            cmd.Parameters.AddWithValue("@Hash", hash);
            cmd.Parameters.AddWithValue("@Salt", salt);

            cmd.ExecuteNonQuery();
        }
    }
}