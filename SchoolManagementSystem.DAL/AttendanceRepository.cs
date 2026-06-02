using Microsoft.Data.SqlClient;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.DAL
{
    public class AttendanceRepository
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        // ================= ADD =================
        public void Add(Attendance a)
        {
            using var conn = db.GetConnection();

            string query = @"
            INSERT INTO Attendance
            (StudentId, AttendanceDate, Status)
            VALUES
            (@StudentId, @Date, @Status)";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@StudentId", a.StudentId);
            cmd.Parameters.AddWithValue("@Date", a.AttendanceDate.Date); // 🔥 مهم
            cmd.Parameters.AddWithValue("@Status", a.Status);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ================= UPDATE =================
        public void Update(Attendance a)
        {
            using var conn = db.GetConnection();

            string query = @"
            UPDATE Attendance
            SET Status = @Status
            WHERE StudentId = @StudentId 
              AND AttendanceDate = @Date";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@StudentId", a.StudentId);
            cmd.Parameters.AddWithValue("@Date", a.AttendanceDate.Date);
            cmd.Parameters.AddWithValue("@Status", a.Status);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ================= EXISTS (IMPORTANT) =================
        public bool Exists(int studentId, DateTime date)
        {
            using var conn = db.GetConnection();

            string query = @"
            SELECT COUNT(*) 
            FROM Attendance
            WHERE StudentId = @id 
              AND AttendanceDate = @date";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", studentId);
            cmd.Parameters.AddWithValue("@date", date.Date);

            conn.Open();

            return (int)cmd.ExecuteScalar() > 0;
        }

        // ================= GET ALL =================
        public List<Attendance> GetAll()
        {
            var list = new List<Attendance>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT a.AttendanceId,
                   a.StudentId,
                   s.FullName,
                   a.AttendanceDate,
                   a.Status
            FROM Attendance a
            INNER JOIN Students s ON a.StudentId = s.StudentId
            ORDER BY a.AttendanceDate DESC";

            using var cmd = new SqlCommand(query, conn);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Attendance
                {
                    AttendanceId = (int)r["AttendanceId"],
                    StudentId = (int)r["StudentId"],
                    StudentName = r["FullName"].ToString(),
                    AttendanceDate = (DateTime)r["AttendanceDate"],
                    Status = r["Status"].ToString()
                });
            }

            return list;
        }

        // ================= GET BY DATE (FIXED) =================
        public List<Attendance> GetByDate(DateTime date)
        {
            var list = new List<Attendance>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT a.AttendanceId,
                   a.StudentId,
                   s.FullName,
                   a.AttendanceDate,
                   a.Status
            FROM Attendance a
            INNER JOIN Students s ON a.StudentId = s.StudentId
            WHERE CAST(a.AttendanceDate AS DATE) = @Date";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Date", date.Date);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Attendance
                {
                    AttendanceId = (int)r["AttendanceId"],
                    StudentId = (int)r["StudentId"],
                    StudentName = r["FullName"].ToString(),
                    AttendanceDate = (DateTime)r["AttendanceDate"],
                    Status = r["Status"].ToString()
                });
            }

            return list;
        }

        // ================= SEARCH =================
        public List<Attendance> SearchByStudentName(string name)
        {
            var list = new List<Attendance>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT a.AttendanceId,
                   a.StudentId,
                   s.FullName,
                   a.AttendanceDate,
                   a.Status
            FROM Attendance a
            INNER JOIN Students s ON a.StudentId = s.StudentId
            WHERE s.FullName LIKE @name
            ORDER BY a.AttendanceDate DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", "%" + name + "%");

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Attendance
                {
                    AttendanceId = (int)r["AttendanceId"],
                    StudentId = (int)r["StudentId"],
                    StudentName = r["FullName"].ToString(),
                    AttendanceDate = (DateTime)r["AttendanceDate"],
                    Status = r["Status"].ToString()
                });
            }

            return list;
        }

        // ================= GET BY STUDENT =================
        public List<Attendance> GetByStudent(int studentId)
        {
            var list = new List<Attendance>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT a.AttendanceId,
                   a.StudentId,
                   s.FullName,
                   a.AttendanceDate,
                   a.Status
            FROM Attendance a
            INNER JOIN Students s ON a.StudentId = s.StudentId
            WHERE a.StudentId = @id
            ORDER BY a.AttendanceDate DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", studentId);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Attendance
                {
                    AttendanceId = (int)r["AttendanceId"],
                    StudentId = (int)r["StudentId"],
                    StudentName = r["FullName"].ToString(),
                    AttendanceDate = (DateTime)r["AttendanceDate"],
                    Status = r["Status"].ToString()
                });
            }

            return list;
        }
        public (int حاضر, int غائب) GetStudentAttendanceStats(int studentId)
        {
            int حاضر = 0;
            int غائب = 0;

            using var conn = db.GetConnection();

            string query = @"
    SELECT Status
    FROM Attendance
    WHERE StudentId = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", studentId);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                string status = r["Status"].ToString();

                if (status == "حاضر")
                    حاضر++;
                else if (status == "غائب")
                    غائب++;
            }

            return (حاضر, غائب);
        }
        // ================= DELETE =================
        public void Delete(int id)
        {
            using var conn = db.GetConnection();

            string query = "DELETE FROM Attendance WHERE AttendanceId = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}