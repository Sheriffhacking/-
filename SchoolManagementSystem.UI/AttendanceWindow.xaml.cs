using SchoolManagementSystem.BLL;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace SchoolManagementSystem.UI
{
    public partial class AttendanceWindow : Window
    {
        private readonly AttendanceService service = new AttendanceService();
        private readonly StudentService studentService = new StudentService();
        private readonly ClassService classService = new ClassService();

        private List<Attendance> list = new();

        public AttendanceWindow()
        {
            InitializeComponent();

            dpDate.SelectedDate = DateTime.Today;

            // ================= LOAD CLASSES =================
            var classes = classService.GetAllClasses().ToList();

            classes.Insert(0, new Class
            {
                ClassId = 0,
                ClassName = "كل الصفوف"
            });

            cmbClass.ItemsSource = classes;
            cmbClass.SelectedValue = 0;

            LoadHistory();
        }

        // ================= LOAD STUDENTS =================
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            DateTime date = dpDate.SelectedDate ?? DateTime.Today;

            List<Student> students;

            // ================= ALL CLASSES =================
            if (cmbClass.SelectedValue == null || (int)cmbClass.SelectedValue == 0)
            {
                students = studentService.GetAllStudents();
            }
            else
            {
                int classId = (int)cmbClass.SelectedValue;
                students = studentService.GetStudentsByClass(classId);
            }

            // ================= CONVERT TO ATTENDANCE =================
            list = students.Select(s => new Attendance
            {
                StudentId = s.StudentId,
                StudentName = s.FullName,

                // 🔥 مهم: من قاعدة البيانات وليس قيمة ثابتة
                ClassName = s.ClassName,

                AttendanceDate = date,
                Status = "حاضر"
            }).ToList();

            dgStudents.ItemsSource = null;
            dgStudents.ItemsSource = list;
        }

        // ================= SAVE =================
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (list == null || list.Count == 0)
            {
                MessageBox.Show("لا يوجد بيانات");
                return;
            }

            foreach (var item in list)
            {
                service.AddOrUpdate(item);
            }

            MessageBox.Show("تم حفظ الحضور بنجاح");
            LoadHistory();
        }

        // ================= SEARCH =================
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
                return;

            var result = service.SearchByStudent(txtSearch.Text);

            dgHistory.ItemsSource = result;

            if (result.Any())
            {
                int studentId = result.First().StudentId;
                var stats = service.GetStats(studentId);

                txtPresentCount.Text = $"الحضور: {stats.حاضر}";
                txtAbsentCount.Text = $"الغياب: {stats.غائب}";
            }
            else
            {
                txtPresentCount.Text = "الحضور: 0";
                txtAbsentCount.Text = "الغياب: 0";
            }
        }

        // ================= HISTORY =================
        private void LoadHistory()
        {
            if (dpDate.SelectedDate != null)
                dgHistory.ItemsSource = service.GetByDate(dpDate.SelectedDate.Value);
            else
                dgHistory.ItemsSource = service.GetAll();
        }

        // ================= MARK ALL PRESENT =================
        private void MarkAllPresent_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list)
                item.Status = "حاضر";

            dgStudents.ItemsSource = null;
            dgStudents.ItemsSource = list;
        }

        // ================= DELETE =================
        private void DeleteAttendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgHistory.SelectedItem == null)
                {
                    MessageBox.Show("اختر سجل الحضور أولاً");
                    return;
                }

                var row = (Attendance)dgHistory.SelectedItem;

                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف سجل الحضور؟",
                    "تأكيد",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    service.Delete(row.AttendanceId);
                    LoadHistory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ================= EXPORT EXCEL =================
        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgHistory.ItemsSource == null)
                {
                    MessageBox.Show("لا يوجد بيانات للتصدير");
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel File|*.xlsx",
                    FileName = "Attendance_Report.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Attendance");

                    worksheet.Cell(1, 1).Value = "اسم الطالب";
                    worksheet.Cell(1, 2).Value = "الصف";
                    worksheet.Cell(1, 3).Value = "التاريخ";
                    worksheet.Cell(1, 4).Value = "الحالة";

                    int row = 2;

                    foreach (Attendance item in dgHistory.ItemsSource)
                    {
                        worksheet.Cell(row, 1).Value = item.StudentName;
                        worksheet.Cell(row, 2).Value = item.ClassName;
                        worksheet.Cell(row, 3).Value = item.AttendanceDate.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 4).Value = item.Status;
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(saveDialog.FileName);
                }

                MessageBox.Show("تم تصدير Excel بنجاح ✔");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التصدير: " + ex.Message);
            }
        }

        // ================= PLACEHOLDER =================
        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "ابحث باسم الطالب")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = Brushes.Black;
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "ابحث باسم الطالب";
                txtSearch.Foreground = Brushes.Gray;
            }
        }

        private void cmbClass_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // ممكن لاحقاً نعمل Auto Load
        }
    }
}