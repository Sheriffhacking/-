using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SchoolManagementSystem.UI
{
    public partial class MainDashboard : Window
    {
        public MainDashboard()
        {
            InitializeComponent();
        }

        private void OpenClasses_Click(object sender, RoutedEventArgs e)
        {
            new ClassWindow().Show();
        }

        private void Analytics_Click(object sender, RoutedEventArgs e)
        {
            new AnalyticsWindow().Show();
        }

        private void OpenStudents_Click(object sender, RoutedEventArgs e)
        {
            new StudentsWindow().Show();
        }

        private void Attendance_Click(object sender, RoutedEventArgs e)
        {
            new AttendanceWindow().Show();
        }

        private void Grades_Click(object sender, RoutedEventArgs e)
        {
            new GradeWindow().Show();
        }

        private void Employees_Click(object sender, RoutedEventArgs e)
        {
            new EmployeeWindow().Show();
        }

        private void Finance_Click(object sender, RoutedEventArgs e)
        {
            new FinanceWindow().Show();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Reports Clicked");
        }

        // ================= BACKUP (بدون WinForms) =================
        private void Backup_Click(object sender, MouseButtonEventArgs e)
        {
            var result = MessageBox.Show(
                "هل تريد إنشاء نسخة احتياطية للنظام؟",
                "تأكيد النسخ الاحتياطي",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // اختيار مجلد الحفظ (WPF فقط)
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Backup File (*.bak)|*.bak",
                    FileName = $"SchoolBackup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.bak"
                };

                if (dialog.ShowDialog() == true)
                {
                    string fullPath = dialog.FileName;

                    string connStr =
 @"Server=(localdb)\MSSQLLocalDB;
Database=master;
Trusted_Connection=True;
TrustServerCertificate=True;
Encrypt=False;";

                    using (SqlConnection con = new SqlConnection(connStr))
                    {
                        con.Open();

                        string query = $@"
BACKUP DATABASE SchoolDB
TO DISK = '{fullPath}'
WITH INIT, FORMAT";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("تم إنشاء النسخة الاحتياطية بنجاح",
                                    "نجاح",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Close_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        private void OpenClassTimetable(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var page = new SchoolManagementSystem.Views.ClassTimetablePage();

                OpenPage(page);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"خطأ بفتح جدول الصفوف: {ex.Message}");
            }
        }

        // =====================================================
        // فتح شاشة جدول المدرسين
        // =====================================================
        private void OpenTeacherTimetable(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var page = new SchoolManagementSystem.Views.TeacherTimetablePage();

                OpenPage(page);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"خطأ بفتح جدول المدرسين: {ex.Message}");
            }
        }

        // =====================================================
        // دالة التنقل داخل الداشبورد (لازم Frame موجود)
        // =====================================================
        private void OpenPage(System.Windows.Controls.Page page)
        {
            // إذا عندك Frame في الداشبورد
            // غيّر الاسم حسب مشروعك (MainFrame / ContentFrame / etc)

            var frame = this.FindName("MainFrame") as System.Windows.Controls.Frame;

            if (frame != null)
            {
                frame.Navigate(page);
            }
            else
            {
                // fallback: افتح نافذة جديدة إذا ما في Frame
                var window = new Window
                {
                    Content = page,
                    Width = 1200,
                    Height = 800,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                window.Show();
            }
        }

        // ================= RESTORE =================
        private void Restore_Click(object sender, MouseButtonEventArgs e)
        {
            var result = MessageBox.Show(
                "استعادة النسخة ستقوم بإغلاق الجلسات الحالية. هل تريد المتابعة؟",
                "تأكيد الاستعادة",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Backup Files (*.bak)|*.bak"
            };

            if (open.ShowDialog() == true)
            {
                try
                {
                    string file = open.FileName;

                    string connStr =
@"Server=(localdb)\MSSQLLocalDB;
Database=master;
Trusted_Connection=True;
TrustServerCertificate=True;
Encrypt=False;";

                    using (SqlConnection con = new SqlConnection(connStr))
                    {
                        con.Open();

                        string query = $@"
ALTER DATABASE SchoolDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE SchoolDB
FROM DISK = '{file}'
WITH REPLACE;

ALTER DATABASE SchoolDB SET MULTI_USER;
";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("تمت استعادة النسخة بنجاح",
                                    "نجاح",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}