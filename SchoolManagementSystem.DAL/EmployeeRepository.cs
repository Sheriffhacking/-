using Microsoft.Data.SqlClient;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.DAL
{
    public class EmployeeRepository
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        // =========================
        // 📥 جلب الموظفين
        // =========================
        public List<Employee> GetAllEmployees()
        {
            List<Employee> list = new List<Employee>();

            using (SqlConnection conn = db.GetConnection())
            {
                string query = "SELECT * FROM Employees";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new Employee
                    {
                        EmployeeId = (int)reader["EmployeeId"],
                        EmployeeName = reader["EmployeeName"].ToString(),
                        NationalId = reader["NationalId"].ToString(),
                        Phone = reader["Phone"] == DBNull.Value ? "" : reader["Phone"].ToString(),
                        JobTitle = reader["JobTitle"] == DBNull.Value ? "" : reader["JobTitle"].ToString(),
                        Salary = reader["Salary"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Salary"]),
                        HireDate = reader["HireDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["HireDate"]),
                        IsActive = reader["IsActive"] != DBNull.Value && (bool)reader["IsActive"]
                    });
                }
            }

            return list;
        }

        // =========================
        // ➕ إضافة موظف
        // =========================
        public void AddEmployee(Employee emp)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
                INSERT INTO Employees
                (EmployeeName, NationalId, Phone, JobTitle, Salary, HireDate, IsActive)
                VALUES
                (@EmployeeName, @NationalId, @Phone, @JobTitle, @Salary, @HireDate, @IsActive)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@EmployeeName", emp.EmployeeName);
                cmd.Parameters.AddWithValue("@NationalId", emp.NationalId);
                cmd.Parameters.AddWithValue("@Phone", emp.Phone ?? "");
                cmd.Parameters.AddWithValue("@JobTitle", emp.JobTitle ?? "");
                cmd.Parameters.AddWithValue("@Salary", emp.Salary);
                cmd.Parameters.AddWithValue("@HireDate", emp.HireDate);
                cmd.Parameters.AddWithValue("@IsActive", emp.IsActive);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        // =========================
        // تعديل موظف
        // =========================

        public void UpdateEmployee(Employee emp)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query = @"
        UPDATE Employees
        SET
            EmployeeName=@EmployeeName,
            Phone=@Phone,
            JobTitle=@JobTitle,
            Salary=@Salary,
            HireDate=@HireDate,
            IsActive=@IsActive
        WHERE EmployeeId=@EmployeeId";

                SqlCommand cmd =
                    new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue(
                    "@EmployeeId", emp.EmployeeId);

                cmd.Parameters.AddWithValue(
                    "@EmployeeName", emp.EmployeeName);

                cmd.Parameters.AddWithValue(
                    "@Phone", emp.Phone ?? "");

                cmd.Parameters.AddWithValue(
                    "@JobTitle", emp.JobTitle ?? "");

                cmd.Parameters.AddWithValue(
                    "@Salary", emp.Salary);

                cmd.Parameters.AddWithValue(
                    "@HireDate", emp.HireDate);

                cmd.Parameters.AddWithValue(
                    "@IsActive", emp.IsActive);

                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // حذف موظف
        // =========================

        public void DeleteEmployee(int id)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                string query =
                    "DELETE FROM Employees WHERE EmployeeId=@EmployeeId";

                SqlCommand cmd =
                    new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue(
                    "@EmployeeId", id);

                conn.Open();

                cmd.ExecuteNonQuery();
            }
        }

    }
}