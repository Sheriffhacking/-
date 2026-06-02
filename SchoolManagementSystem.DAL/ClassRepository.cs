using Microsoft.Data.SqlClient;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.DAL
{
    public class ClassRepository
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        // =========================
        // GET ALL CLASSES (ERP STANDARD)
        // =========================
        public List<Class> GetAllClasses()
        {
            List<Class> list = new List<Class>();

            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
                    SELECT 
                        ClassId,
                        ClassName,
                        ClassTeacherName
                    FROM Classes
                    ORDER BY ClassName";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new Class
                    {
                        ClassId = Convert.ToInt32(reader["ClassId"]),
                        ClassName = reader["ClassName"]?.ToString(),
                        ClassTeacherName = reader["ClassTeacherName"]?.ToString()
                    });
                }
            }

            return list;
        }

        // =========================
        // ADD CLASS
        // =========================
        public void Add(Class cls)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
                    INSERT INTO Classes (ClassName, ClassTeacherName)
                    VALUES (@ClassName, @ClassTeacherName)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ClassName", cls.ClassName);
                cmd.Parameters.AddWithValue("@ClassTeacherName",
                    (object?)cls.ClassTeacherName ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // UPDATE CLASS
        // =========================
        public void Update(Class cls)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
                    UPDATE Classes
                    SET ClassName = @ClassName,
                        ClassTeacherName = @ClassTeacherName
                    WHERE ClassId = @ClassId";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ClassId", cls.ClassId);
                cmd.Parameters.AddWithValue("@ClassName", cls.ClassName);
                cmd.Parameters.AddWithValue("@ClassTeacherName",
                    (object?)cls.ClassTeacherName ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // DELETE CLASS (SAFE)
        // =========================
        public void Delete(int id)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                // تحقق من وجود طلاب
                string checkQuery = @"
                    SELECT COUNT(*) 
                    FROM Students 
                    WHERE ClassId = @ClassId";

                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@ClassId", id);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    throw new Exception("❌ لا يمكن حذف الصف لأنه يحتوي على طلاب");
                }

                // حذف
                string deleteQuery = @"
                    DELETE FROM Classes 
                    WHERE ClassId = @ClassId";

                SqlCommand cmd = new SqlCommand(deleteQuery, conn);
                cmd.Parameters.AddWithValue("@ClassId", id);

                cmd.ExecuteNonQuery();
            }
        }
    }
}