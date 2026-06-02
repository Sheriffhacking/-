using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.DAL
{
    public class AnalyticsRepository
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        // ================= TOP STUDENTS =================
        public List<dynamic> GetTopStudents()
        {
            var list = new List<dynamic>();

            using var conn = db.GetConnection();

            string query = @"
                SELECT TOP 10
                    s.StudentId,
                    s.FullName,
                    c.ClassName,
                    AVG(g.Score) AS AvgScore
                FROM Grades g
                JOIN Students s ON s.StudentId = g.StudentId
                JOIN Classes c ON s.ClassId = c.ClassId
                GROUP BY s.StudentId, s.FullName, c.ClassName
                ORDER BY AvgScore DESC";

            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new
                {
                    StudentId = Convert.ToInt32(r["StudentId"]),
                    Student = r["FullName"].ToString(),
                    Class = r["ClassName"].ToString(),
                    Average = Convert.ToDecimal(r["AvgScore"])
                });
            }

            return list;
        }

        // ================= WEAK STUDENTS =================
        public List<dynamic> GetWeakStudents()
        {
            var list = new List<dynamic>();

            using var conn = db.GetConnection();
            string query = @"
    SELECT TOP 10
        s.StudentId,
        s.FullName,
        c.ClassName,
         COALESCE(AVG(g.Score), 0) AS AvgScore
    FROM Students s
    LEFT JOIN Grades g ON s.StudentId = g.StudentId
    JOIN Classes c ON s.ClassId = c.ClassId
    GROUP BY s.StudentId, s.FullName, c.ClassName
    ORDER BY AvgScore ASC";

            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new
                {
                    StudentId = Convert.ToInt32(r["StudentId"]),
                    Student = r["FullName"].ToString(),
                    Class = r["ClassName"].ToString(),
                    Average = Convert.ToDecimal(r["AvgScore"])
                });
            }

            return list;
        }

        // ================= CLASS RANKING =================
        public List<dynamic> GetClassRanking()
        {
            var list = new List<dynamic>();

            using var conn = db.GetConnection();

            string query = @"
                SELECT
                    c.ClassId,
                    c.ClassName,
                    COUNT(s.StudentId) AS StudentsCount,
                    AVG(g.Score) AS AverageScore
                FROM Classes c
                LEFT JOIN Students s ON c.ClassId = s.ClassId
                LEFT JOIN Grades g ON s.StudentId = g.StudentId
                GROUP BY c.ClassId, c.ClassName
                ORDER BY AverageScore DESC";

            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new
                {
                    ClassId = Convert.ToInt32(r["ClassId"]),
                    Class = r["ClassName"].ToString(),
                    StudentsCount = Convert.ToInt32(r["StudentsCount"]),
                    Average = r["AverageScore"] == DBNull.Value ? 0 : Convert.ToDecimal(r["AverageScore"])
                });
            }

            return list;
        }

        // ================= SUBJECT RANKING =================
        public List<dynamic> GetSubjectRanking()
        {
            var list = new List<dynamic>();

            using var conn = db.GetConnection();

            string query = @"
                SELECT
                    sub.SubjectName,
                    COUNT(g.Score) AS ExamsCount,
                    AVG(g.Score) AS AverageScore
                FROM Grades g
                JOIN Subjects sub ON g.SubjectId = sub.SubjectId
                GROUP BY sub.SubjectName
                ORDER BY AverageScore DESC";

            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new
                {
                    Subject = r["SubjectName"].ToString(),
                    ExamsCount = Convert.ToInt32(r["ExamsCount"]),
                    Average = Convert.ToDecimal(r["AverageScore"])
                });
            }

            return list;
        }

        // ================= DTO =================
        public class SchoolSummaryDto
        {
            public decimal SchoolAverage { get; set; }
            public decimal MaxScore { get; set; }
            public decimal MinScore { get; set; }
        }

        // ================= SCHOOL SUMMARY =================
        public SchoolSummaryDto GetSchoolSummary()
        {
            using var conn = db.GetConnection();

            string query = @"
                SELECT
                    (SELECT COUNT(*) FROM Students) AS TotalStudents,
                    (SELECT COUNT(*) FROM Classes) AS TotalClasses,
                    (SELECT COUNT(*) FROM Subjects) AS TotalSubjects,
                    (SELECT AVG(Score) FROM Grades) AS SchoolAverage,
                    (SELECT MAX(Score) FROM Grades) AS MaxScore,
                    (SELECT MIN(Score) FROM Grades) AS MinScore";

            conn.Open();
            using var cmd = new SqlCommand(query, conn);
            using var r = cmd.ExecuteReader();

            if (r.Read())
            {
                return new SchoolSummaryDto
                {
                    SchoolAverage = r["SchoolAverage"] == DBNull.Value ? 0 : Convert.ToDecimal(r["SchoolAverage"]),
                    MaxScore = r["MaxScore"] == DBNull.Value ? 0 : Convert.ToDecimal(r["MaxScore"]),
                    MinScore = r["MinScore"] == DBNull.Value ? 0 : Convert.ToDecimal(r["MinScore"])
                };
            }

            return new SchoolSummaryDto();
        }
    }
}