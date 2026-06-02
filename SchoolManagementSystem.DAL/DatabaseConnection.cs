using Microsoft.Data.SqlClient;

namespace SchoolManagementSystem.DAL
{
    public class DatabaseConnection
    {
        private readonly string _connectionString =
            "Data Source=(LocalDB)\\MSSQLLocalDB;" +
            "Initial Catalog=SchoolDB;" +
            "Integrated Security=True;" +
            "TrustServerCertificate=True;" +
            "Encrypt=False;" +
            "Connection Timeout=30;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}