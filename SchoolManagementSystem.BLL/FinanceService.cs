using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagementSystem.BLL
{
    public class FinanceService
    {
        private readonly TransactionRepository repo =
            new TransactionRepository();

        // ================= ADD =================
        public void Add(Transaction t)
        {
            repo.Add(t);
        }

        // ================= DELETE =================
        public void Delete(int id)
        {
            repo.Delete(id);
        }

        // ================= GET ALL =================
        public List<Transaction> GetAll()
        {
            return repo.GetAll();
        }

        public List<Transaction> GetAllTransactions()
        {
            return repo.GetAll();
        }

        // ================= FILTER =================
        public List<Transaction> Filter(
            DateTime from,
            DateTime to,
            string type)
        {
            var data = repo.FilterByDate(from, to);

            if (!string.IsNullOrWhiteSpace(type)
                && type != "الكل")
            {
                data = data
                    .Where(x => x.Type == type)
                    .ToList();
            }

            return data;
        }

        // ================= INCOME =================
        public decimal Income(DateTime from, DateTime to)
        {
            var data = repo.FilterByDate(from, to);

            return data
                .Where(x => x.Type == "قبض")
                .Sum(x => x.Amount);
        }

        // ================= EXPENSES =================
        public decimal Expenses(DateTime from, DateTime to)
        {
            var data = repo.FilterByDate(from, to);

            return data
                .Where(x => x.Type == "صرف")
                .Sum(x => x.Amount);
        }

        // ================= SALARIES =================
        public decimal Salaries(DateTime from, DateTime to)
        {
            var data = repo.FilterByDate(from, to);

            return data
                .Where(x => x.Type == "راتب")
                .Sum(x => x.Amount);
        }

        // ================= PROFIT =================
        public decimal Profit(DateTime from, DateTime to)
        {
            return Income(from, to)
                 - Expenses(from, to)
                 - Salaries(from, to);
        }

        // ================= STUDENT PAYMENT STATUS =================
        public bool IsStudentPaid(
            int studentId,
            int feeTypeId,
            int month,
            string year)
        {
            var data = repo.GetAll();

            return data.Any(x =>
                x.StudentId == studentId &&
                x.FeeTypeId == feeTypeId &&
                x.StudyMonth == month &&
                x.AcademicYear == year &&
                x.Type == "قبض");
        }

        // ================= UNPAID STUDENTS =================
        public List<Transaction> GetUnpaidStudents(
            int? classId,
            int feeTypeId,
            int month,
            string year,
            List<int> allStudentIds)
        {
            var data = repo.GetAll();

            var paidStudents = data
                .Where(x =>
                    x.Type == "قبض" &&
                    x.FeeTypeId == feeTypeId &&
                    x.StudyMonth == month &&
                    x.AcademicYear == year)
                .Select(x => x.StudentId)
                .ToHashSet();

            return data
                .Where(x =>
                    x.Type == "طالب" && // فقط placeholder
                    allStudentIds.Contains(x.StudentId ?? 0) &&
                    !paidStudents.Contains(x.StudentId))
                .ToList();
        }
    }
}