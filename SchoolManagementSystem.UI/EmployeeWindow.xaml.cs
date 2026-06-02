using SchoolManagementSystem.BLL;
using SchoolManagementSystem.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SchoolManagementSystem.UI
{
    public partial class EmployeeWindow : Window
    {
        // =========================
        // Services
        // =========================

        private EmployeeService employeeService =
            new EmployeeService();

        // =========================
        // Selected Employee
        // =========================

        private int selectedId = 0;

        // =========================
        // Constructor
        // =========================

        public EmployeeWindow()
        {
            InitializeComponent();

            dpHireDate.SelectedDate = DateTime.Now;

            LoadEmployees();
        }

        // =========================
        // تحميل الموظفين
        // =========================

        private void LoadEmployees()
        {
            try
            {
                dgEmployees.ItemsSource =
                    employeeService.GetAllEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "خطأ أثناء تحميل الموظفين\n" + ex.Message);
            }
        }

        // =========================
        // إضافة موظف
        // =========================

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtEmployeeName.Text))
                {
                    MessageBox.Show("أدخل اسم الموظف");
                    return;
                }

                Employee emp = new Employee
                {
                    EmployeeName = txtEmployeeName.Text,

                    NationalId = txtNationalId.Text,

                    Phone = txtPhone.Text,

                    JobTitle = txtJobTitle.Text,

                    Salary =
                        decimal.TryParse(txtSalary.Text, out decimal salary)
                        ? salary : 0,

                    HireDate =
                        dpHireDate.SelectedDate ?? DateTime.Now,

                    IsActive =
                        chkActive.IsChecked == true
                };

                employeeService.AddEmployee(emp);

                MessageBox.Show("تم إضافة الموظف بنجاح");

                LoadEmployees();

                ClearData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "خطأ أثناء إضافة الموظف\n" + ex.Message);
            }
        }

        // =========================
        // تعديل موظف
        // =========================

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("اختر موظف للتعديل");
                return;
            }

            try
            {
                Employee emp = new Employee
                {
                    EmployeeId = selectedId,

                    EmployeeName = txtEmployeeName.Text,

                    NationalId = txtNationalId.Text,

                    Phone = txtPhone.Text,

                    JobTitle = txtJobTitle.Text,

                    Salary =
                        decimal.TryParse(txtSalary.Text, out decimal salary)
                        ? salary : 0,

                    HireDate =
                        dpHireDate.SelectedDate ?? DateTime.Now,

                    IsActive =
                        chkActive.IsChecked == true
                };

                employeeService.UpdateEmployee(emp);

                MessageBox.Show("تم تعديل الموظف بنجاح");

                LoadEmployees();

                ClearData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "خطأ أثناء تعديل الموظف\n" + ex.Message);
            }
        }

        // =========================
        // حذف موظف
        // =========================

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("اختر موظف للحذف");
                return;
            }

            try
            {
                MessageBoxResult result =
                    MessageBox.Show(
                        "هل تريد حذف الموظف؟",
                        "تأكيد الحذف",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    employeeService.DeleteEmployee(selectedId);

                    MessageBox.Show("تم حذف الموظف");

                    LoadEmployees();

                    ClearData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "خطأ أثناء حذف الموظف\n" + ex.Message);
            }
        }

        // =========================
        // اختيار موظف
        // =========================

        private void dgEmployees_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            try
            {
                if (dgEmployees.SelectedItem is Employee emp)
                {
                    selectedId = emp.EmployeeId;

                    txtEmployeeName.Text =
                        emp.EmployeeName;

                    txtNationalId.Text =
                        emp.NationalId;

                    txtPhone.Text =
                        emp.Phone;

                    txtJobTitle.Text =
                        emp.JobTitle;

                    txtSalary.Text =
                        emp.Salary.ToString();

                    dpHireDate.SelectedDate =
                        emp.HireDate;

                    chkActive.IsChecked =
                        emp.IsActive;
                }
            }
            catch
            {
            }
        }


        // =========================
        // تنظيف الحقول
        // =========================

        private void ClearData()
        {
            txtEmployeeName.Clear();

            txtNationalId.Clear();

            txtPhone.Clear();

            txtJobTitle.Clear();

            txtSalary.Clear();

            dpHireDate.SelectedDate =
                DateTime.Now;

            chkActive.IsChecked = true;

            selectedId = 0;
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearData();
        }
    }
}