using Microsoft.Data.SqlClient;
using System;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.DAL
{
    public class UserRepository
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        public User GetUserByUsername(string username)
        {
            using var conn = db.GetConnection();
            conn.Open();

            string query = "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", username);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    UserId = (int)reader["UserId"],
                    Username = reader["Username"].ToString(),
                    PasswordHash = (byte[])reader["PasswordHash"],
                    PasswordSalt = (byte[])reader["PasswordSalt"],
                    Role = reader["Role"].ToString()
                };
            }

            return null;
        }

        public void UpdatePassword(int userId, byte[] hash, byte[] salt)
        {
            using var conn = db.GetConnection();
            conn.Open();

            string query = @"
                UPDATE Users 
                SET PasswordHash = @Hash, PasswordSalt = @Salt 
                WHERE UserId = @Id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Hash", hash);
            cmd.Parameters.AddWithValue("@Salt", salt);
            cmd.Parameters.AddWithValue("@Id", userId);

            cmd.ExecuteNonQuery();
        }
    }
}