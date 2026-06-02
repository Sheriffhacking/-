using Microsoft.Data.SqlClient;
using SchoolManagementSystem.BLL;
using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SchoolManagementSystem.UI
{
    public partial class GradeWindow : Window
    {
        // ===== SERVICES =====
        private readonly StudentService _studentService = new StudentService();
        private readonly SubjectService _subjectService = new SubjectService();
        private readonly GradeService _gradeService = new GradeService();
        private readonly ClassService _classService = new ClassService();

        // ===== STATE =====
        // قائمة الدرجات الحالية المعروضة في الجدول
        private List<Grade> _currentGrades = new List<Grade>();

        // ================================================================
        // CONSTRUCTOR
        // ================================================================
        public GradeWindow()
        {
            InitializeComponent();
            LoadDropdowns();
            dpDate.SelectedDate = DateTime.Today;

            // عرض التاريخ الحالي في الـ Header
            txtCurrentDate.Text = DateTime.Today.ToString(
                "dddd، dd MMMM yyyy",
                new CultureInfo("ar-SA"));
        }

        // ================================================================
        // LOAD DROPDOWNS
        // ================================================================
        private void LoadDropdowns()
        {
            cmbClass.ItemsSource = _classService.GetAllClasses();
            cmbSubject.ItemsSource = _subjectService.GetAllSubjects();
        }

        // ================================================================
        // LOAD STUDENTS  →  تحميل طلاب الصف لإدخال الدرجات
        // ================================================================
        private void LoadStudents_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClass.SelectedValue == null)
            {
                ShowWarning("يرجى اختيار الصف الدراسي أولاً");
                return;
            }

            int classId = Convert.ToInt32(cmbClass.SelectedValue);
            var students = _studentService.GetStudentsByClass(classId);

            if (!students.Any())
            {
                ShowInfo("لا يوجد طلاب مسجّلون في هذا الصف");
                return;
            }

            // تحضير قائمة فارغة لإدخال الدرجات
            _currentGrades = students
                .Select(s => new Grade
                {
                    StudentId = s.StudentId,
                    StudentName = s.FullName,
                    Score = 0,
                    Notes = ""
                })
                .ToList();

            dgGrades.ItemsSource = _currentGrades;
            UpdateDashboard(_currentGrades);
        }

        // ================================================================
        // SAVE GRADES  →  حفظ الدرجات في قاعدة البيانات
        // ================================================================
        private void SaveGrades_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFilters()) return;

            if (!_currentGrades.Any())
            {
                ShowWarning("قم بتحميل الطلاب أولاً عبر زر «تحميل الطلاب»");
                return;
            }

            int subjectId = Convert.ToInt32(cmbSubject.SelectedValue);
            string examType = GetComboText(cmbExamType);
            string semester = GetComboText(cmbSemester);
            DateTime date = dpDate.SelectedDate ?? DateTime.Today;

            int saved = 0;
            int skipped = 0;

            try
            {
                foreach (var g in _currentGrades)
                {
                    // تخطي الدرجات خارج النطاق المقبول
                    if (g.Score < 0 || g.Score > 100)
                    {
                        skipped++;
                        continue;
                    }

                    g.SubjectId = subjectId;
                    g.ExamType = examType;
                    g.Semester = semester;
                    g.GradeDate = date;

                    _gradeService.Add(g);
                    saved++;
                }

                string msg = $"✅ تم حفظ {saved} درجة بنجاح.";
                if (skipped > 0)
                    msg += $"\n⚠️ تم تخطي {skipped} سطر (الدرجة خارج نطاق 0–100).";

                ShowInfo(msg, "نتيجة الحفظ");
                RefreshGrades();
            }
            catch (Exception ex)
            {
                ShowError("خطأ أثناء الحفظ:\n" + ex.Message);
            }
        }
        private readonly DatabaseConnection db = new DatabaseConnection();
        // ================================================================
        // DELETE GRADE  →  حذف درجة محددة
        // ================================================================
        private void DeleteGrade_Click(object sender, RoutedEventArgs e)
        {
            // التحقق من أن العنصر المحدد هو Grade وليس dynamic
            if (dgGrades.SelectedItem is not Grade selected)
            {
                ShowWarning("يرجى تحديد صف من الجدول أولاً");
                return;
            }

            if (selected.GradeId <= 0)
            {
                ShowWarning("هذه الدرجة لم تُحفظ بعد في قاعدة البيانات");
                return;
            }

            var confirm = MessageBox.Show(
                $"هل أنت متأكد من حذف درجة الطالب:\n{selected.StudentName}؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _gradeService.Delete(selected.GradeId);
                ShowInfo("تم حذف الدرجة بنجاح", "تم");
                RefreshGrades();
            }
            catch (Exception ex)
            {
                ShowError("خطأ أثناء الحذف:\n" + ex.Message);
            }
        }

        // ================================================================
        // CLASS RESULTS  →  عرض نتائج الصف بالمادة المحددة
        // ================================================================
        private void ClassResults_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClass.SelectedValue == null)
            {
                ShowWarning("يرجى اختيار الصف الدراسي أولاً");
                return;
            }

            int classId = Convert.ToInt32(cmbClass.SelectedValue);

            List<Grade> data;

            // إذا لم تُختر مادة → جلب كل المواد
            if (cmbSubject.SelectedValue == null)
            {
                data = _gradeService.GetByClass(classId);
            }
            else
            {
                int subjectId = Convert.ToInt32(cmbSubject.SelectedValue);
                data = _gradeService.GetClassSubjectGrades(classId, subjectId);
            }

            _currentGrades = data;
            dgGrades.ItemsSource = data;
            UpdateDashboard(data);
        }

        // ================================================================
        // SEARCH  →  البحث عن طالب بالاسم
        // ================================================================
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string input = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                ShowWarning("يرجى إدخال اسم الطالب للبحث");
                return;
            }

            // تقسيم النص المدخل إلى كلمات منفصلة
            var keywords = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.ToLower())
                .ToList();

            // البحث عن الطلاب الذين يحتوي اسمهم على جميع الكلمات
            var matches = _studentService
                .GetAllStudents()
                .Where(s => keywords.All(k => s.FullName.ToLower().Contains(k)))
                .ToList();

            if (!matches.Any())
            {
                ShowInfo($"لم يُعثر على طالب بالكلمات: {input}", "نتيجة البحث");
                return;
            }

            // جمع درجات جميع النتائج
            var data = matches
                .SelectMany(s => _gradeService.GetByStudent(s.StudentId))
                .ToList();

            _currentGrades = data;
            dgGrades.ItemsSource = data;
            UpdateDashboard(data);
        }
        // ================= ALL GRADES BY CLASS (كل المواد) =================
        public List<Grade> GetByClass(int classId)
        {
            var list = new List<Grade>();

            using var conn = db.GetConnection();

            string query = @"
        SELECT
            g.GradeId,
            g.StudentId,
            s.FullName       AS StudentName,
            c.ClassName,
            sub.SubjectName,
            g.ExamType,
            g.Semester,
            g.Score,
            g.Notes,
            g.GradeDate
        FROM Grades g
        INNER JOIN Students s   ON g.StudentId  = s.StudentId
        INNER JOIN Classes  c   ON s.ClassId    = c.ClassId
        INNER JOIN Subjects sub ON g.SubjectId  = sub.SubjectId
        WHERE s.ClassId = @classId
        ORDER BY s.FullName, sub.SubjectName";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@classId", classId);

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Grade
                {
                    GradeId = Convert.ToInt32(r["GradeId"]),
                    StudentId = Convert.ToInt32(r["StudentId"]),
                    StudentName = r["StudentName"].ToString(),
                    ClassName = r["ClassName"].ToString(),
                    SubjectName = r["SubjectName"].ToString(),
                    ExamType = r["ExamType"].ToString(),
                    Semester = r["Semester"].ToString(),
                    Score = Convert.ToDecimal(r["Score"]),
                    Notes = r["Notes"].ToString(),
                    GradeDate = Convert.ToDateTime(r["GradeDate"])
                });
            }

            return list;
        }

        // ================================================================
        // DASHBOARD  →  تحديث بطاقات الإحصاء
        // ================================================================
        private void UpdateDashboard(List<Grade> data)
        {
            if (data == null || !data.Any())
            {
                txtStudentsCount.Text = "0";
                txtAverage.Text = "—";
                txtHighest.Text = "—";
                txtPassRate.Text = "—";
                return;
            }

            int count = data.Count;
            double average = (double)data.Average(x => x.Score);
            double highest = (double)data.Max(x => x.Score);
            int passCount = data.Count(x => x.Score >= 60);
            double passRate = (passCount * 100.0) / count;

            txtStudentsCount.Text = count.ToString();
            txtAverage.Text = average.ToString("0.0");
            txtHighest.Text = highest.ToString("0.0");
            txtPassRate.Text = passRate.ToString("0") + "%";
        }

        // ================================================================
        // HELPERS
        // ================================================================

        /// <summary>إعادة تحميل نتائج الصف بعد الحفظ أو الحذف</summary>
        private void RefreshGrades()
        {
            if (cmbClass.SelectedValue != null && cmbSubject.SelectedValue != null)
                ClassResults_Click(null, null);
        }

        /// <summary>التحقق من اكتمال حقول الفلتر الضرورية قبل الحفظ</summary>
        private bool ValidateFilters()
        {
            if (cmbSubject.SelectedValue == null)
            {
                ShowWarning("يرجى اختيار المادة الدراسية");
                return false;
            }
            if (cmbExamType.SelectedItem == null)
            {
                ShowWarning("يرجى اختيار نوع الاختبار");
                return false;
            }
            if (cmbSemester.SelectedItem == null)
            {
                ShowWarning("يرجى اختيار الفصل الدراسي");
                return false;
            }
            return true;
        }

        /// <summary>استخراج النص من ComboBoxItem المحدد</summary>
        private static string GetComboText(ComboBox combo)
            => (combo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

        // --- MessageBox Helpers ---
        private static void ShowWarning(string msg, string title = "تنبيه")
            => MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        private static void ShowInfo(string msg, string title = "معلومة")
            => MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);

        private static void ShowError(string msg, string title = "خطأ")
            => MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
        // ================================================================
        // EXPORT TO EXCEL
        // ================================================================
        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_currentGrades == null || !_currentGrades.Any())
            {
                ShowWarning("لا توجد بيانات للتصدير. قم بتحميل النتائج أولاً.");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "حفظ ملف Excel",
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"نتائج_الصف_{DateTime.Today:yyyy-MM-dd}"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                using var wb = new ClosedXML.Excel.XLWorkbook();
                var ws = wb.Worksheets.Add("النتائج");

                // RTL
                ws.RightToLeft = true;

                // ===== HEADERS =====
                var headers = new[]
                {
            "اسم الطالب", "الصف", "المادة", "نوع الاختبار",
            "الفصل", "الدرجة", "التقدير", "التاريخ", "ملاحظات"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#1E3A5F");
                    cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                }

                // ===== DATA =====
                for (int i = 0; i < _currentGrades.Count; i++)
                {
                    var g = _currentGrades[i];
                    int row = i + 2;

                    ws.Cell(row, 1).Value = g.StudentName;
                    ws.Cell(row, 2).Value = g.ClassName;
                    ws.Cell(row, 3).Value = g.SubjectName;
                    ws.Cell(row, 4).Value = g.ExamType;
                    ws.Cell(row, 5).Value = g.Semester;
                    ws.Cell(row, 6).Value = (double)g.Score;
                    ws.Cell(row, 7).Value = g.Evaluation;
                    ws.Cell(row, 8).Value = g.GradeDate.ToString("dd/MM/yyyy");
                    ws.Cell(row, 9).Value = g.Notes;

                    // تلوين صف التقدير
                    var scoreCell = ws.Cell(row, 6);
                    scoreCell.Style.Fill.BackgroundColor = g.Score >= 90
                        ? ClosedXML.Excel.XLColor.FromHtml("#D5F5E3")
                        : g.Score >= 60
                            ? ClosedXML.Excel.XLColor.FromHtml("#FEF9E7")
                            : ClosedXML.Excel.XLColor.FromHtml("#FADBD8");
                }

                // ===== FORMAT =====
                ws.Columns().AdjustToContents();
                ws.Range(1, 1, 1, headers.Length).SetAutoFilter();

                wb.SaveAs(dialog.FileName);

                var open = MessageBox.Show(
                    "✅ تم التصدير بنجاح!\nهل تريد فتح الملف الآن؟",
                    "تم", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (open == MessageBoxResult.Yes)
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(dialog.FileName)
                        { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowError("خطأ أثناء التصدير:\n" + ex.Message);
            }
        }
        // ================================================================
        // CLEAR FIELDS  →  تفريغ جميع الحقول وإعادة الضبط
        // ================================================================
        private void ClearFields_Click(object sender, RoutedEventArgs e)
        {
            // تفريغ الـ ComboBoxes
            cmbClass.SelectedIndex = -1;
            cmbSubject.SelectedIndex = -1;
            cmbExamType.SelectedIndex = -1;
            cmbSemester.SelectedIndex = -1;

            // إعادة التاريخ لليوم
            dpDate.SelectedDate = DateTime.Today;

            // تفريغ خانة البحث
            txtSearch.Clear();

            // تفريغ الجدول والقائمة الداخلية
            _currentGrades = new List<Grade>();
            dgGrades.ItemsSource = null;

            // إعادة ضبط Dashboard
            UpdateDashboard(null);
        }
    }
}