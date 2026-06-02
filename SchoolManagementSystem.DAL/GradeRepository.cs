using Microsoft.Data.SqlClient;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.DAL
{
    public class GradeRepository
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        // ================= ADD =================
        public void Add(Grade g)
        {
            using var conn = db.GetConnection();

            string query = @"
            INSERT INTO Grades
            (StudentId, SubjectId, ExamType, Semester, Score, Notes, GradeDate)
            VALUES
            (@StudentId, @SubjectId, @ExamType, @Semester, @Score, @Notes, @GradeDate)";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@StudentId", g.StudentId);
            cmd.Parameters.AddWithValue("@SubjectId", g.SubjectId);
            cmd.Parameters.AddWithValue("@ExamType", g.ExamType ?? "");
            cmd.Parameters.AddWithValue("@Semester", g.Semester ?? "");
            cmd.Parameters.AddWithValue("@Score", g.Score);
            cmd.Parameters.AddWithValue("@Notes", g.Notes ?? "");
            cmd.Parameters.AddWithValue("@GradeDate", g.GradeDate);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ================= DELETE =================
        public void Delete(int gradeId)
        {
            using var conn = db.GetConnection();

            string query = "DELETE FROM Grades WHERE GradeId = @id";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", gradeId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ================= BY STUDENT =================
        public List<Grade> GetByStudent(int studentId)
        {
            var list = new List<Grade>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT
                g.GradeId,
                g.StudentId,
                g.SubjectId,
                s.FullName,
                sub.SubjectName,
                g.ExamType,
                g.Semester,
                g.Score,
                g.Notes,
                g.GradeDate
            FROM Grades g
            INNER JOIN Students s ON g.StudentId = s.StudentId
            INNER JOIN Subjects sub ON g.SubjectId = sub.SubjectId
            WHERE g.StudentId = @id
            ORDER BY g.GradeDate DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", studentId);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Grade
                {
                    GradeId = Convert.ToInt32(r["GradeId"]),
                    StudentId = Convert.ToInt32(r["StudentId"]),
                    StudentName = r["FullName"].ToString(),
                    SubjectName = r["SubjectName"].ToString(),
                    ExamType = r["ExamType"].ToString(),
                    Semester = r["Semester"].ToString(),
                    Score = Convert.ToDecimal(r["Score"]),
                    Notes = r["Notes"].ToString(),
                    GradeDate = Convert.ToDateTime(r["GradeDate"])
                });
            }

            return list;
        }

        // ================= BY CLASS =================
        public List<Grade> GetByClass(int classId)
        {
            var list = new List<Grade>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT
                g.GradeId,
                g.StudentId,
                s.FullName AS StudentName,
                g.SubjectId,
                sub.SubjectName,
                g.ExamType,
                g.Semester,
                g.Score,
                g.Notes,
                g.GradeDate
            FROM Grades g
            INNER JOIN Students s ON g.StudentId = s.StudentId
            INNER JOIN Subjects sub ON g.SubjectId = sub.SubjectId
            WHERE s.ClassId = @classId
            ORDER BY g.GradeDate DESC";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@classId", classId);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Grade
                {
                    GradeId = Convert.ToInt32(r["GradeId"]),
                    StudentId = Convert.ToInt32(r["StudentId"]),
                    StudentName = r["StudentName"].ToString(),
                    SubjectName = r["SubjectName"].ToString(),
                    ExamType = r["ExamType"].ToString(),
                    Semester = r["Semester"].ToString(),
                    Score = Convert.ToDecimal(r["Score"]),
                    Notes = r["Notes"].ToString(),
                    GradeDate = Convert.ToDateTime(r["GradeDate"])
                });
            }

            return list;
        }

        // ================= CLASS + SUBJECT =================
        public List<Grade> GetClassSubjectGrades(int classId, int subjectId)
        {
            var list = new List<Grade>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT
                g.GradeId,
                g.StudentId,
                s.FullName AS StudentName,
                c.ClassName,
                sub.SubjectName,
                g.ExamType,
                g.Semester,
                g.Score,
                g.Notes,
                g.GradeDate
            FROM Grades g
            INNER JOIN Students s ON g.StudentId = s.StudentId
            INNER JOIN Classes c ON s.ClassId = c.ClassId
            INNER JOIN Subjects sub ON g.SubjectId = sub.SubjectId
            WHERE s.ClassId = @classId
              AND g.SubjectId = @subjectId
            ORDER BY g.Score DESC";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@classId", classId);
            cmd.Parameters.AddWithValue("@subjectId", subjectId);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Grade
                {
                    GradeId = Convert.ToInt32(r["GradeId"]),
                    StudentId = Convert.ToInt32(r["StudentId"]),
                    StudentName = r["StudentName"].ToString(),
                    ClassName = r["ClassName"].ToString(),
                    SubjectName = r["SubjectName"].ToString(),
                    ExamType = r["ExamType"].ToString(),
                    Semester = r["Semester"].ToString(),
                    Score = Convert.ToDecimal(r["Score"]),
                    Notes = r["Notes"].ToString(),
                    GradeDate = Convert.ToDateTime(r["GradeDate"])
                });
            }

            return list;
        }
        public List<dynamic> GetAllGradesWithDetails(int classId)
        {
            var list = new List<dynamic>();

            using var conn = db.GetConnection();

            string query = @"
    SELECT 
        s.FullName AS StudentName,
        c.ClassName,
        sub.SubjectName,
        g.Score,
        g.ExamType,
        g.Semester,
        g.GradeDate
    FROM Grades g
    INNER JOIN Students s ON g.StudentId = s.StudentId
    INNER JOIN Classes c ON s.ClassId = c.ClassId
    INNER JOIN Subjects sub ON g.SubjectId = sub.SubjectId
    WHERE s.ClassId = @classId
    ORDER BY s.FullName";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@classId", classId);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new
                {
                    StudentName = r["StudentName"].ToString(),
                    ClassName = r["ClassName"].ToString(),
                    SubjectName = r["SubjectName"].ToString(),
                    Score = Convert.ToDecimal(r["Score"]),
                    ExamType = r["ExamType"].ToString(),
                    Semester = r["Semester"].ToString(),
                    GradeDate = Convert.ToDateTime(r["GradeDate"])
                });
            }

            return list;
        }

        // ================= EXPORT =================
        public List<dynamic> GetGradesByClassAndSubject(int classId, int subjectId)
        {
            var list = new List<dynamic>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT 
                s.FullName AS StudentName,
                c.ClassName,
                sub.SubjectName,
                g.Score,
                g.ExamType,
                g.Semester,
                g.GradeDate
            FROM Grades g
            INNER JOIN Students s ON g.StudentId = s.StudentId
            INNER JOIN Classes c ON s.ClassId = c.ClassId
            INNER JOIN Subjects sub ON g.SubjectId = sub.SubjectId
            WHERE s.ClassId = @classId
              AND g.SubjectId = @subjectId
            ORDER BY s.FullName";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@classId", classId);
            cmd.Parameters.AddWithValue("@subjectId", subjectId);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new
                {
                    StudentName = r["StudentName"].ToString(),
                    ClassName = r["ClassName"].ToString(),
                    SubjectName = r["SubjectName"].ToString(),
                    Score = Convert.ToDecimal(r["Score"]),
                    ExamType = r["ExamType"].ToString(),
                    Semester = r["Semester"].ToString(),
                    GradeDate = Convert.ToDateTime(r["GradeDate"])
                });
            }

            return list;
        }
    }
}