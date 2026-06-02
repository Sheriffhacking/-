using SchoolManagementSystem.BLL;
using SchoolManagementSystem.Models;
using System;
using System.Linq;
using System.Windows;

namespace SchoolManagementSystem.UI
{
    public partial class ClassWindow : Window
    {
        // الخدمات
        private ClassService classService = new ClassService();
        private StudentService studentService = new StudentService();

        // الصف المحدد
        private int selectedId = 0;

        public ClassWindow()
        {
            InitializeComponent();

            LoadClasses();
        }

        // ==========================
        // تحميل الصفوف
        // ==========================
        private void LoadClasses()
        {
            dgClasses.ItemsSource = null;

            dgClasses.ItemsSource = classService.GetAllClasses();
        }

        // ==========================
        // حفظ صف جديد
        // ==========================
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtClassName.Text))
                {
                    MessageBox.Show(
                        "الرجاء إدخال اسم الصف",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                Class cls = new Class
                {
                    ClassName = txtClassName.Text,
                    ClassTeacherName = txtTeacherName.Text
                };

                classService.AddClass(cls);

                MessageBox.Show(
                    "تم إضافة الصف بنجاح",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadClasses();

                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ==========================
        // تعديل صف
        // ==========================
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedId == 0)
                {
                    MessageBox.Show(
                        "اختر صف للتعديل",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                Class cls = new Class
                {
                    ClassId = selectedId,
                    ClassName = txtClassName.Text,
                    ClassTeacherName = txtTeacherName.Text
                };

                classService.UpdateClass(cls);

                MessageBox.Show(
                    "تم تعديل الصف بنجاح",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadClasses();

                ClearFields();

                selectedId = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ==========================
        // حذف صف
        // ==========================
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedId == 0)
                {
                    MessageBox.Show(
                        "اختر صف للحذف",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف الصف؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    classService.DeleteClass(selectedId);

                    MessageBox.Show(
                        "تم حذف الصف بنجاح",
                        "نجاح",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    LoadClasses();

                    ClearFields();

                    dgStudentsInClass.ItemsSource = null;

                    txtCount.Text = "";

                    selectedId = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ==========================
        // عند اختيار صف
        // ==========================
        private void dgClasses_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgClasses.SelectedItem is Class cls)
            {
                selectedId = cls.ClassId;

                txtClassName.Text = cls.ClassName;

                txtTeacherName.Text = cls.ClassTeacherName;

                var students = studentService
                    .GetAllStudents()
                    .Where(s => s.ClassId == cls.ClassId)
                    .ToList();

                dgStudentsInClass.ItemsSource = students;

                txtCount.Text = $"عدد الطلاب: {students.Count}";
            }
        }

        // ==========================
        // تنظيف الحقول
        // ==========================
        private void ClearFields()
        {
            txtClassName.Clear();

            txtTeacherName.Clear();

            dgClasses.SelectedItem = null;
        }
    }
}