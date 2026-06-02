// ============================================================
// DatabaseHelper.cs
// ============================================================

using Microsoft.Data.SqlClient;

namespace SchoolManagementSystem.Helpers
{
    public static class DatabaseHelper
    {
        // ⚠️ عدّل هذا السطر فقط
        public static string ConnectionString =

            "Data Source=(LocalDB)\\MSSQLLocalDB;" +
            "Initial Catalog=SchoolDB;" +
            "Integrated Security=True;" +
            "TrustServerCertificate=True;" +
            "Encrypt=False;" +
            "Connection Timeout=30;";

        public static SqlConnection GetConnection() =>
            new SqlConnection(ConnectionString);
    }
}
