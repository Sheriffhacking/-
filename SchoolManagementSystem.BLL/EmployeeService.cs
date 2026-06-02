using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System.Collections.Generic;

namespace SchoolManagementSystem.BLL
{
    public class EmployeeService
    {
        private EmployeeRepository repo =
            new EmployeeRepository();

        // =========================
        // جلب الموظفين
        // =========================

        public List<Employee> GetAllEmployees()
        {
            return repo.GetAllEmployees();
        }

        // =========================
        // إضافة موظف
        // =========================

        public void AddEmployee(Employee emp)
        {
            repo.AddEmployee(emp);
        }

        // =========================
        // تعديل موظف
        // =========================

        public void UpdateEmployee(Employee emp)
        {
            repo.UpdateEmployee(emp);
        }

        // =========================
        // حذف موظف
        // =========================

        public void DeleteEmployee(int id)
        {
            repo.DeleteEmployee(id);
        }
    }
}