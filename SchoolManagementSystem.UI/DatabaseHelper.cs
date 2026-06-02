using Microsoft.Data.SqlClient;
using System;
using System.Configuration;

namespace SchoolManagementSystem
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString =
            ConfigurationManager.ConnectionStrings["SchoolDBConnection"]?.ConnectionString
            ?? throw new Exception("Connection string 'SchoolDBConnection' not found.");

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}