using Microsoft.Data.SqlClient;
using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Helpers;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.Repositories
{
    public class TimetableRepository
    {
        // ────────────────────────────────────────────────────
        // Classes
        // ────────────────────────────────────────────────────
        public List<Class> GetClasses()
        {
            var list = new List<Class>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(
                "SELECT ClassId, ClassName FROM Classes ORDER BY ClassName", conn);

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Class
                {
                    ClassId = (int)r["ClassId"],
                    ClassName = r["ClassName"].ToString()
                });
            }

            return list;
        }

        // ────────────────────────────────────────────────────
        // Subjects
        // ────────────────────────────────────────────────────
        public List<Subject> GetSubjects()
        {
            var list = new List<Subject>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            var sql = @"
                SELECT SubjectId, SubjectName,
                       CASE WHEN COL_LENGTH('Subjects','SubjectColor') IS NOT NULL
                            THEN SubjectColor ELSE '#E0E0E0' END AS SubjectColor
                FROM Subjects
                WHERE IsActive = 1
                ORDER BY SubjectName";

            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Subject
                {
                    SubjectId = (int)r["SubjectId"],
                    SubjectName = r["SubjectName"].ToString(),
                    ColorHex = r["SubjectColor"]?.ToString() ?? "#E0E0E0"
                });
            }

            return list;
        }

        // ────────────────────────────────────────────────────
        // Employees (FIXED 100%)
        // ────────────────────────────────────────────────────
        public List<SchoolManagementSystem.Models.Employee> GetEmployees()
        {
            var list = new List<SchoolManagementSystem.Models.Employee>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(
                "SELECT EmployeeId, EmployeeName FROM Employees WHERE IsActive=1 ORDER BY EmployeeName",
                conn);

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new SchoolManagementSystem.Models.Employee
                {
                    EmployeeId = (int)r["EmployeeId"],
                    EmployeeName = r["EmployeeName"].ToString()
                });
            }

            return list;
        }

        // ────────────────────────────────────────────────────
        // Load timetable for class
        // ────────────────────────────────────────────────────
        public List<ClassTimetableEntry> GetByClass(int classId)
        {
            var list = new List<ClassTimetableEntry>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT ct.Id, ct.ClassId, ct.SubjectId, ct.TeacherId,
                       ct.DayOrder, ct.PeriodNumber,
                       ISNULL(s.SubjectName, '')  AS SubjectName,
                       ISNULL(e.EmployeeName, '') AS TeacherName,
                       ISNULL(s.SubjectColor, '#E0E0E0') AS SubjectColor
                FROM ClassTimetable ct
                LEFT JOIN Subjects s ON ct.SubjectId = s.SubjectId
                LEFT JOIN Employees e ON ct.TeacherId = e.EmployeeId
                WHERE ct.ClassId = @cid
                ORDER BY ct.DayOrder, ct.PeriodNumber", conn);

            cmd.Parameters.AddWithValue("@cid", classId);

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new ClassTimetableEntry
                {
                    Id = (int)r["Id"],
                    ClassId = (int)r["ClassId"],
                    SubjectId = r["SubjectId"] == DBNull.Value ? null : (int?)r["SubjectId"],
                    TeacherId = r["TeacherId"] == DBNull.Value ? null : (int?)r["TeacherId"],
                    DayOrder = (int)r["DayOrder"],
                    PeriodNumber = (int)r["PeriodNumber"],
                    SubjectName = r["SubjectName"].ToString(),
                    TeacherName = r["TeacherName"].ToString(),
                    SubjectColor = r["SubjectColor"].ToString()
                });
            }

            return list;
        }
        public void DeleteByClassId(int classId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(
                "DELETE FROM ClassTimetable WHERE ClassId = @ClassId", conn);

            cmd.Parameters.AddWithValue("@ClassId", classId);
            cmd.ExecuteNonQuery();
        }
        public List<ClassTimetableEntry> GetByTeacher(int teacherId)
        {
            var list = new List<ClassTimetableEntry>();

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
        SELECT ct.Id, ct.ClassId, ct.SubjectId, ct.TeacherId,
               ct.DayOrder, ct.PeriodNumber,
               ISNULL(s.SubjectName, '') AS SubjectName,
               ISNULL(c.ClassName, '')   AS TeacherName,
               ISNULL(s.SubjectColor, '#E0E0E0') AS SubjectColor
        FROM ClassTimetable ct
        LEFT JOIN Subjects s ON ct.SubjectId = s.SubjectId
        LEFT JOIN Classes  c ON ct.ClassId = c.ClassId
        WHERE ct.TeacherId = @tid
        ORDER BY ct.DayOrder, ct.PeriodNumber", conn);

            cmd.Parameters.AddWithValue("@tid", teacherId);

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new ClassTimetableEntry
                {
                    Id = (int)r["Id"],
                    ClassId = (int)r["ClassId"],
                    SubjectId = r["SubjectId"] == DBNull.Value ? null : (int?)r["SubjectId"],
                    TeacherId = r["TeacherId"] == DBNull.Value ? null : (int?)r["TeacherId"],
                    DayOrder = (int)r["DayOrder"],
                    PeriodNumber = (int)r["PeriodNumber"],
                    SubjectName = r["SubjectName"].ToString(),
                    TeacherName = r["TeacherName"].ToString(),
                    SubjectColor = r["SubjectColor"].ToString()
                });
            }

            return list;
        }
        // ────────────────────────────────────────────────────
        // Upsert
        // ────────────────────────────────────────────────────
        public void UpsertCell(int classId, int dayOrder, int periodNumber,
                               int? subjectId, int? teacherId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var checkCmd = new SqlCommand(@"
                SELECT Id FROM ClassTimetable
                WHERE ClassId=@cid AND DayOrder=@day AND PeriodNumber=@per", conn);

            checkCmd.Parameters.AddWithValue("@cid", classId);
            checkCmd.Parameters.AddWithValue("@day", dayOrder);
            checkCmd.Parameters.AddWithValue("@per", periodNumber);

            var existing = checkCmd.ExecuteScalar();

            if (existing != null)
            {
                using var upd = new SqlCommand(@"
                    UPDATE ClassTimetable
                    SET SubjectId=@sid, TeacherId=@tid
                    WHERE Id=@id", conn);

                upd.Parameters.AddWithValue("@sid", (object?)subjectId ?? DBNull.Value);
                upd.Parameters.AddWithValue("@tid", (object?)teacherId ?? DBNull.Value);
                upd.Parameters.AddWithValue("@id", (int)existing);

                upd.ExecuteNonQuery();
            }
            else
            {
                using var ins = new SqlCommand(@"
                    INSERT INTO ClassTimetable
                    (ClassId,SubjectId,TeacherId,DayOrder,PeriodNumber)
                    VALUES (@cid,@sid,@tid,@day,@per)", conn);

                ins.Parameters.AddWithValue("@cid", classId);
                ins.Parameters.AddWithValue("@sid", (object?)subjectId ?? DBNull.Value);
                ins.Parameters.AddWithValue("@tid", (object?)teacherId ?? DBNull.Value);
                ins.Parameters.AddWithValue("@day", dayOrder);
                ins.Parameters.AddWithValue("@per", periodNumber);

                ins.ExecuteNonQuery();
            }
        }
    }
}