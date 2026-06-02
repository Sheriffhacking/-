using Microsoft.Data.SqlClient;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.DAL
{
    public class TransactionRepository
    {
        private readonly DatabaseConnection db =
            new DatabaseConnection();

        // =========================
        // ADD
        // =========================
        public void Add(Transaction t)
        {
            using var conn = db.GetConnection();

            string query = @"
            INSERT INTO Transactions
            (Type, StudentId, EmployeeId, Amount,
             TotalRequiredAmount, RemainingAmount,
             Description, Date,
             FeeTypeId, AcademicYear, StudyMonth, PaymentMethod)
            VALUES
            (@Type, @StudentId, @EmployeeId, @Amount,
             @TotalRequiredAmount, @RemainingAmount,
             @Description, @Date,
             @FeeTypeId, @AcademicYear, @StudyMonth, @PaymentMethod)";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Type",
                t.Type ?? "");

            cmd.Parameters.AddWithValue("@StudentId",
                (object?)t.StudentId ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@EmployeeId",
                (object?)t.EmployeeId ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@Amount",
                t.Amount);

            // ✅ الإضافتان الجديدتان
            cmd.Parameters.AddWithValue("@TotalRequiredAmount",
                t.TotalRequiredAmount);

            cmd.Parameters.AddWithValue("@RemainingAmount",
                t.RemainingAmount);

            cmd.Parameters.AddWithValue("@Description",
                (object?)t.Description ?? "");

            cmd.Parameters.AddWithValue("@Date",
                t.Date);

            cmd.Parameters.AddWithValue("@FeeTypeId",
                (object?)t.FeeTypeId ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@AcademicYear",
                (object?)t.AcademicYear ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@StudyMonth",
                (object?)t.StudyMonth ?? DBNull.Value);

            cmd.Parameters.AddWithValue("@PaymentMethod",
                (object?)t.PaymentMethod ?? DBNull.Value);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // =========================
        // DELETE
        // =========================
        public void Delete(int id)
        {
            using var conn = db.GetConnection();

            using var cmd = new SqlCommand(
                "DELETE FROM Transactions WHERE TransactionId=@id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // =========================
        // GET ALL
        // =========================
        public List<Transaction> GetAll()
        {
            var list = new List<Transaction>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT 
                t.TransactionId,
                t.Type,
                t.Amount,
                t.TotalRequiredAmount,
                t.RemainingAmount,
                t.Description,
                t.Date,

                t.FeeTypeId,
                f.FeeName,

                t.AcademicYear,
                t.StudyMonth,
                t.PaymentMethod,

                t.StudentId,
                s.FullName   AS StudentName,

                t.EmployeeId,
                e.EmployeeName

            FROM Transactions t

            LEFT JOIN Students  s ON t.StudentId  = s.StudentId
            LEFT JOIN Employees e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN FeeTypes  f ON t.FeeTypeId  = f.FeeTypeId

            ORDER BY t.Date DESC";

            using var cmd = new SqlCommand(query, conn);

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Transaction
                {
                    TransactionId = Convert.ToInt32(r["TransactionId"]),
                    Type = r["Type"].ToString(),
                    Amount = Convert.ToDecimal(r["Amount"]),

                    // ✅ الإضافتان الجديدتان
                    TotalRequiredAmount = r["TotalRequiredAmount"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(r["TotalRequiredAmount"]),

                    RemainingAmount = r["RemainingAmount"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(r["RemainingAmount"]),

                    Description = r["Description"].ToString(),
                    Date = Convert.ToDateTime(r["Date"]),

                    StudentId = r["StudentId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(r["StudentId"]),

                    StudentName = r["StudentName"].ToString(),

                    EmployeeId = r["EmployeeId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(r["EmployeeId"]),

                    EmployeeName = r["EmployeeName"].ToString(),

                    FeeTypeId = r["FeeTypeId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(r["FeeTypeId"]),

                    FeeName = r["FeeName"].ToString(),
                    AcademicYear = r["AcademicYear"].ToString(),

                    StudyMonth = r["StudyMonth"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(r["StudyMonth"]),

                    PaymentMethod = r["PaymentMethod"].ToString()
                });
            }

            return list;
        }

        // =========================
        // FILTER BY DATE
        // =========================
        public List<Transaction> FilterByDate(DateTime from, DateTime to)
        {
            var list = new List<Transaction>();

            using var conn = db.GetConnection();

            string query = @"
            SELECT 
                t.TransactionId,
                t.Type,
                t.Amount,
                t.TotalRequiredAmount,
                t.RemainingAmount,
                t.Description,
                t.Date,

                t.FeeTypeId,
                f.FeeName,

                t.AcademicYear,
                t.StudyMonth,
                t.PaymentMethod,

                t.StudentId,
                s.FullName   AS StudentName,

                t.EmployeeId,
                e.EmployeeName

            FROM Transactions t

            LEFT JOIN Students  s ON t.StudentId  = s.StudentId
            LEFT JOIN Employees e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN FeeTypes  f ON t.FeeTypeId  = f.FeeTypeId

            WHERE t.Date >= @from 
              AND t.Date <  DATEADD(day, 1, @to)

            ORDER BY t.Date DESC";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.Add("@from", System.Data.SqlDbType.DateTime)
                .Value = from.Date;

            cmd.Parameters.Add("@to", System.Data.SqlDbType.DateTime)
                .Value = to.Date;

            conn.Open();

            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Transaction
                {
                    TransactionId = Convert.ToInt32(r["TransactionId"]),
                    Type = r["Type"].ToString(),
                    Amount = Convert.ToDecimal(r["Amount"]),

                    // ✅ الإضافتان الجديدتان
                    TotalRequiredAmount = r["TotalRequiredAmount"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(r["TotalRequiredAmount"]),

                    RemainingAmount = r["RemainingAmount"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(r["RemainingAmount"]),

                    Description = r["Description"].ToString(),
                    Date = Convert.ToDateTime(r["Date"]),

                    StudentId = r["StudentId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(r["StudentId"]),

                    StudentName = r["StudentName"].ToString(),

                    EmployeeId = r["EmployeeId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(r["EmployeeId"]),

                    EmployeeName = r["EmployeeName"].ToString(),

                    FeeTypeId = r["FeeTypeId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(r["FeeTypeId"]),

                    FeeName = r["FeeName"].ToString(),
                    AcademicYear = r["AcademicYear"].ToString(),

                    StudyMonth = r["StudyMonth"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(r["StudyMonth"]),

                    PaymentMethod = r["PaymentMethod"].ToString()
                });
            }

            return list;
        }
    }
}