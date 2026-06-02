using Microsoft.Data.SqlClient;
using SchoolManagementSystem.Models;
using System.Collections.Generic;

namespace SchoolManagementSystem.DAL
{
    public class SubjectRepository
    {
        private readonly DatabaseConnection db =
            new DatabaseConnection();
        public string ColorHex { get; set; } = "#E0E0E0";
        public List<Subject> GetAll()
        {
            var list = new List<Subject>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT *
            FROM Subjects
            WHERE IsActive = 1
            ORDER BY SubjectName";

            using var cmd =
                new SqlCommand(query, conn);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Subject
                {
                    SubjectId = (int)r["SubjectId"],
                    SubjectName = r["SubjectName"].ToString(),
                    MaxMark = (decimal)r["MaxMark"],
                    IsActive = (bool)r["IsActive"]
                });
            }

            return list;
        }
    }
}