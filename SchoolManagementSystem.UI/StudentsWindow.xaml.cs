using ClosedXML.Excel;
using Microsoft.Win32;
using SchoolManagementSystem.BLL;
using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SchoolManagementSystem.UI
{
    public partial class StudentsWindow : Window
    {
        // ─── Services ───────────────────────────────────────────────────
        private readonly ClassService classService = new ClassService();
        private readonly StudentService service = new StudentService();
        private List<Student> students = new();

        // ─── State ──────────────────────────────────────────────────────
        private int selectedStudentId = 0;
        private byte[] currentPhotoBytes = null;
        private List<string> pendingAttachPaths = new List<string>();

        // ════════════════════════════════════════════════════════════════
        public StudentsWindow()
        {
            InitializeComponent();
            LoadClasses();
            LoadStudents();
        }

        // ================================================================
        // DATA LOAD
        // ================================================================

        private void LoadClasses()
        {
            var classes = classService.GetAllClasses();
            cmbClasses.ItemsSource = classes;
            cmbClasses.DisplayMemberPath = "ClassName";
            cmbClasses.SelectedValuePath = "ClassId";
        }

        private void LoadStudents()
        {
            students = service.GetAllStudents();
            dgStudents.ItemsSource = students;
        }

        // ================================================================
        // SEARCH
        // ================================================================

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string search = txtSearch.Text?.Trim().ToLower() ?? "";

                dgStudents.ItemsSource = service.GetAllStudents()
                    .Where(s =>
                        (s.FullName ?? "").ToLower().Contains(search) ||
                        (s.NationalId ?? "").ToLower().Contains(search) ||
                        (s.Phone ?? "").ToLower().Contains(search) ||
                        (s.ClassName ?? "").ToLower().Contains(search))
                    .ToList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ================================================================
        // PHOTO  — pick, preview, remove
        // ================================================================

        private void PickPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "اختر صورة الطالب",
                Filter = "صور|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                currentPhotoBytes = File.ReadAllBytes(dlg.FileName);
                ShowPhotoPreview(currentPhotoBytes);
            }
            catch (Exception ex) { MessageBox.Show("خطأ في تحميل الصورة: " + ex.Message); }
        }

        private void RemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            currentPhotoBytes = null;
            imgStudentPhoto.Source = null;
            imgStudentPhoto.Visibility = Visibility.Collapsed;
            pnlPhotoPlaceholder.Visibility = Visibility.Visible;
        }

        private void ShowPhotoPreview(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(bytes);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                imgStudentPhoto.Source = bmp;
                imgStudentPhoto.Visibility = Visibility.Visible;
                pnlPhotoPlaceholder.Visibility = Visibility.Collapsed;
            }
            catch { /* silently ignore bad image data */ }
        }

        // ================================================================
        // ATTACHMENTS
        // ================================================================

        private void Attachments_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "اختر مرفقات الطالب",
                Filter = "ملفات مدعومة|*.jpg;*.jpeg;*.png;*.pdf;*.zip;*.rar;*.docx;*.xlsx",
                Multiselect = true
            };

            if (dlg.ShowDialog() != true) return;

            foreach (var path in dlg.FileNames)
            {
                if (!pendingAttachPaths.Contains(path))
                    pendingAttachPaths.Add(path);
            }

            UpdateAttachLabel();
        }

        private void UpdateAttachLabel()
        {
            int savedCount = selectedStudentId > 0
                ? service.GetAttachmentCount(selectedStudentId)
                : 0;
            int pendingCount = pendingAttachPaths.Count;
            int total = savedCount + pendingCount;

            lblAttachCount.Text = total == 0
                ? "لا توجد مرفقات"
                : $"{total} مرفق";
        }

        // ================================================================
        // SAVE / UPDATE
        // ================================================================

        private void SaveStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                { MessageBox.Show("الرجاء إدخال اسم الطالب"); return; }

                if (string.IsNullOrWhiteSpace(txtNationalId.Text))
                { MessageBox.Show("الرجاء إدخال رقم الهوية"); return; }

                if (cmbClasses.SelectedValue == null)
                { MessageBox.Show("الرجاء اختيار الصف"); return; }

                if (!decimal.TryParse(txtGpa.Text, out decimal gpa))
                { MessageBox.Show("المعدل غير صحيح"); return; }

                string gender = (cmbGender.SelectedItem is ComboBoxItem item)
                    ? item.Content.ToString()
                    : "";

                var student = new Student
                {
                    StudentId = selectedStudentId,
                    FullName = txtName.Text.Trim(),
                    NationalId = txtNationalId.Text.Trim(),
                    ClassId = (int)cmbClasses.SelectedValue,
                    DateOfBirth = dpBirth.SelectedDate ?? DateTime.Now,
                    RegistrationDate = DateTime.Now,
                    Gender = gender,
                    Phone = txtPhone.Text.Trim(),
                    GuardianName = txtGuardianName.Text.Trim(),
                    GuardianNationalId = txtGuardianId.Text.Trim(),
                    PreviousClass = txtPreviousClass.Text.Trim(),
                    PreviousGPA = gpa,
                    IsActive = true,
                    PhotoData = currentPhotoBytes
                };

                if (selectedStudentId == 0)
                {
                    int newId = service.AddStudent(student);
                    selectedStudentId = newId;
                    SavePendingAttachments(newId);
                    MessageBox.Show("تم إضافة الطالب بنجاح ✔");
                }
                else
                {
                    service.UpdateStudent(student);
                    SavePendingAttachments(selectedStudentId);
                    MessageBox.Show("تم تعديل بيانات الطالب ✔");
                }

                LoadStudents();
                ClearFields();
                selectedStudentId = 0;
                btnUpdate.IsEnabled = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void SavePendingAttachments(int studentId)
        {
            foreach (var path in pendingAttachPaths)
            {
                try
                {
                    var attach = new StudentAttachment
                    {
                        StudentId = studentId,
                        FileName = Path.GetFileName(path),
                        FileData = File.ReadAllBytes(path),
                        UploadDate = DateTime.Now
                    };
                    service.AddAttachment(attach);
                }
                catch { /* skip individual bad file */ }
            }
            pendingAttachPaths.Clear();
            UpdateAttachLabel();
        }

        // ================================================================
        // EXPORT EXCEL  — fixed: moved to StudentsWindow, uses local list
        // ================================================================

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (students == null || students.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات للتصدير");
                    return;
                }

                var dlg = new SaveFileDialog
                {
                    Filter = "Excel File (*.xlsx)|*.xlsx",
                    FileName = $"Students_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (dlg.ShowDialog() != true) return;

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Students");

                // Headers
                string[] headers =
                {
                    "ID", "الاسم الكامل", "رقم الهوية", "الصف",
                    "الجنس", "الهاتف", "ولي الأمر", "المعدل", "نشط"
                };

                for (int i = 0; i < headers.Length; i++)
                    ws.Cell(1, i + 1).Value = headers[i];

                var headerRange = ws.Range(1, 1, 1, headers.Length);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var s in students)
                {
                    ws.Cell(row, 1).Value = s.StudentId;
                    ws.Cell(row, 2).Value = s.FullName;
                    ws.Cell(row, 3).Value = s.NationalId;
                    ws.Cell(row, 4).Value = s.ClassName;
                    ws.Cell(row, 5).Value = s.Gender;
                    ws.Cell(row, 6).Value = s.Phone;
                    ws.Cell(row, 7).Value = s.GuardianName;
                    ws.Cell(row, 8).Value = s.PreviousGPA;
                    ws.Cell(row, 9).Value = s.IsActive ? "نعم" : "لا";
                    row++;
                }

                ws.Columns().AdjustToContents();
                ws.RightToLeft = true;

                wb.SaveAs(dlg.FileName);
                MessageBox.Show("تم التصدير بنجاح ✔");
            }
            catch (Exception ex) { MessageBox.Show("خطأ في التصدير: " + ex.Message); }
        }

        // ================================================================
        // IMPORT EXCEL  — fixed: resolves ClassId from ClassName
        // ================================================================

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Filter = "Excel File (*.xlsx)|*.xlsx"
                };

                if (dlg.ShowDialog() != true) return;

                // Load classes once for lookup
                var allClasses = classService.GetAllClasses();

                using var wb = new XLWorkbook(dlg.FileName);
                var ws = wb.Worksheet(1);

                var rows = ws.RangeUsed().RowsUsed().Skip(1); // skip header row

                int added = 0;
                int skipped = 0;

                foreach (var row in rows)
                {
                    try
                    {
                        string className = row.Cell(4).GetString().Trim();

                        // Resolve ClassId from ClassName — skips row if class not found
                        var matchedClass = allClasses
                            .FirstOrDefault(c => c.ClassName.Equals(className,
                                StringComparison.OrdinalIgnoreCase));

                        if (matchedClass == null)
                        {
                            skipped++;
                            continue;
                        }

                        // Parse IsActive: accepts "نعم" / "Yes" / "true" / "1"
                        string activeStr = row.Cell(9).GetString().Trim().ToLower();
                        bool isActive = activeStr == "نعم"
                                     || activeStr == "yes"
                                     || activeStr == "true"
                                     || activeStr == "1";

                        // Parse GPA safely
                        decimal.TryParse(row.Cell(8).GetString(), out decimal gpa);

                        var student = new Student
                        {
                            FullName = row.Cell(2).GetString().Trim(),
                            NationalId = row.Cell(3).GetString().Trim(),
                            ClassName = className,
                            ClassId = matchedClass.ClassId,   // ← الإصلاح الرئيسي
                            Gender = row.Cell(5).GetString().Trim(),
                            Phone = row.Cell(6).GetString().Trim(),
                            GuardianName = row.Cell(7).GetString().Trim(),
                            PreviousGPA = gpa,
                            IsActive = isActive,
                            RegistrationDate = DateTime.Now
                        };

                        service.AddStudent(student);
                        added++;
                    }
                    catch
                    {
                        skipped++;
                    }
                }

                LoadStudents();

                string msg = $"تم استيراد {added} طالب بنجاح ✔";
                if (skipped > 0)
                    msg += $"\nتم تخطي {skipped} سجل (صف غير موجود أو بيانات ناقصة)";

                MessageBox.Show(msg);
            }
            catch (Exception ex) { MessageBox.Show("خطأ في الاستيراد: " + ex.Message); }
        }

        // ================================================================
        // SELECTION CHANGED
        // ================================================================

        private void dgStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgStudents.SelectedItem is not Student s) return;

                selectedStudentId = s.StudentId;

                txtName.Text = s.FullName;
                txtNationalId.Text = s.NationalId;
                cmbClasses.SelectedValue = s.ClassId;
                dpBirth.SelectedDate = s.DateOfBirth;
                txtPhone.Text = s.Phone;
                txtGuardianName.Text = s.GuardianName;
                txtGuardianId.Text = s.GuardianNationalId;
                txtPreviousClass.Text = s.PreviousClass;
                txtGpa.Text = s.PreviousGPA.ToString();

                foreach (ComboBoxItem item in cmbGender.Items)
                {
                    if (item.Content.ToString() == s.Gender)
                    { cmbGender.SelectedItem = item; break; }
                }

                currentPhotoBytes = s.PhotoData;
                if (currentPhotoBytes != null && currentPhotoBytes.Length > 0)
                    ShowPhotoPreview(currentPhotoBytes);
                else
                    RemovePhoto_Click(null, null);

                pendingAttachPaths.Clear();
                UpdateAttachLabel();

                btnUpdate.IsEnabled = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ================================================================
        // DOUBLE-CLICK
        // ================================================================

        private void dgStudents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgStudents.SelectedItem is Student s)
                OpenStudentProfile(s);
        }

        private void OpenStudentProfile(Student s)
        {
            var profileWin = new StudentProfileWindow(s);
            profileWin.Owner = this;
            profileWin.ShowDialog();
        }

        // ================================================================
        // UPDATE
        // ================================================================

        private void UpdateStudent_Click(object sender, RoutedEventArgs e)
            => SaveStudent_Click(sender, e);

        // ================================================================
        // DELETE
        // ================================================================

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgStudents.SelectedItem is not Student s)
                { MessageBox.Show("اختر طالباً للحذف"); return; }

                var result = MessageBox.Show(
                    $"هل أنت متأكد من حذف الطالب:\n{s.FullName}؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                service.DeleteStudent(s.StudentId);
                LoadStudents();
                ClearFields();
                selectedStudentId = 0;
                btnUpdate.IsEnabled = false;
                MessageBox.Show("تم حذف الطالب بنجاح ✔");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ================================================================
        // NEW
        // ================================================================

        private void NewStudent_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            selectedStudentId = 0;
            dgStudents.SelectedItem = null;
            btnUpdate.IsEnabled = false;
        }

        // ================================================================
        // CLEAR FIELDS
        // ================================================================

        private void ClearFields()
        {
            txtName.Text = "";
            txtNationalId.Text = "";
            cmbClasses.SelectedIndex = -1;
            dpBirth.SelectedDate = null;
            cmbGender.SelectedIndex = -1;
            txtPhone.Text = "";
            txtGuardianName.Text = "";
            txtGuardianId.Text = "";
            txtPreviousClass.Text = "";
            txtGpa.Text = "";

            currentPhotoBytes = null;
            imgStudentPhoto.Source = null;
            imgStudentPhoto.Visibility = Visibility.Collapsed;
            pnlPhotoPlaceholder.Visibility = Visibility.Visible;

            pendingAttachPaths.Clear();
            UpdateAttachLabel();
        }
    }


    // ════════════════════════════════════════════════════════════════════
    //  STUDENT PROFILE WINDOW
    // ════════════════════════════════════════════════════════════════════

    public class StudentProfileWindow : Window
    {
        private readonly Student _student;

        public StudentProfileWindow(Student student)
        {
            _student = student;
            Title = $"ملف الطالب — {student.FullName}";
            Width = 560;
            Height = 680;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FlowDirection = FlowDirection.RightToLeft;
            Background = new SolidColorBrush(Color.FromRgb(11, 20, 37));

            BuildUI();
        }

        private void BuildUI()
        {
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(20)
            };

            var root = new StackPanel { Margin = new Thickness(0) };
            scroll.Content = root;
            Content = scroll;

            // ── Header card ──
            var headerCard = MakeCard();
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var photoBorder = new Border
            {
                Width = 100,
                Height = 100,
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(Color.FromRgb(22, 34, 56)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(30, 51, 82)),
                BorderThickness = new Thickness(2)
            };

            if (_student.PhotoData != null && _student.PhotoData.Length > 0)
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(_student.PhotoData);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                photoBorder.Child = new Image
                {
                    Source = bmp,
                    Stretch = Stretch.UniformToFill,
                    Clip = new RectangleGeometry(new Rect(0, 0, 100, 100), 14, 14)
                };
            }
            else
            {
                photoBorder.Child = new TextBlock
                {
                    Text = "👤",
                    FontSize = 40,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            Grid.SetColumn(photoBorder, 0);
            headerGrid.Children.Add(photoBorder);

            var infoStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 0, 0)
            };
            infoStack.Children.Add(new TextBlock
            {
                Text = _student.FullName,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(241, 245, 249))
            });
            infoStack.Children.Add(new TextBlock
            {
                Text = _student.ClassName ?? "—",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Margin = new Thickness(0, 4, 0, 0)
            });

            var activeBadge = new Border
            {
                Background = new SolidColorBrush(_student.IsActive
                    ? Color.FromRgb(5, 46, 22) : Color.FromRgb(46, 5, 5)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            activeBadge.Child = new TextBlock
            {
                Text = _student.IsActive ? "✔ نشط" : "✖ غير نشط",
                FontSize = 11,
                Foreground = new SolidColorBrush(_student.IsActive
                    ? Color.FromRgb(52, 211, 153) : Color.FromRgb(248, 113, 113))
            };
            infoStack.Children.Add(activeBadge);

            Grid.SetColumn(infoStack, 1);
            headerGrid.Children.Add(infoStack);
            headerCard.Child = headerGrid;
            root.Children.Add(headerCard);

            // ── Details card ──
            var detailCard = MakeCard();
            var detailStack = new StackPanel();

            AddDetailRow(detailStack, "🪪  رقم الهوية", _student.NationalId);
            AddDetailRow(detailStack, "📅  تاريخ الميلاد", _student.DateOfBirth.ToString("yyyy/MM/dd"));
            AddDetailRow(detailStack, "⚧  الجنس", _student.Gender);
            AddDetailRow(detailStack, "📞  الهاتف", _student.Phone);
            AddDetailRow(detailStack, "👨‍👦  ولي الأمر", _student.GuardianName);
            AddDetailRow(detailStack, "🪪  هوية ولي الأمر", _student.GuardianNationalId);
            AddDetailRow(detailStack, "🏫  الصف السابق", _student.PreviousClass);
            AddDetailRow(detailStack, "📊  المعدل السابق", _student.PreviousGPA.ToString("F2"));
            AddDetailRow(detailStack, "📆  تاريخ التسجيل", _student.RegistrationDate.ToString("yyyy/MM/dd"));

            detailCard.Child = detailStack;
            root.Children.Add(detailCard);

            // ── Action buttons ──
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var btnExport = MakeActionButton("📸  تصدير كصورة", Color.FromRgb(37, 99, 235));
            btnExport.Click += (s, e) => ExportAsImage(scroll);

            var btnPrint = MakeActionButton("🖨  طباعة مباشرة", Color.FromRgb(5, 150, 105));
            btnPrint.Click += (s, e) => PrintProfile();

            var btnClose = MakeActionButton("✖  إغلاق", Color.FromRgb(55, 65, 81));
            btnClose.Click += (s, e) => Close();

            btnPanel.Children.Add(btnExport);
            btnPanel.Children.Add(new Border { Width = 10 });
            btnPanel.Children.Add(btnPrint);
            btnPanel.Children.Add(new Border { Width = 10 });
            btnPanel.Children.Add(btnClose);

            root.Children.Add(btnPanel);
        }

        // ── Helpers ──────────────────────────────────────────────────

        private Border MakeCard() => new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(15, 30, 53)),
            CornerRadius = new CornerRadius(16),
            BorderBrush = new SolidColorBrush(Color.FromRgb(30, 51, 82)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 12)
        };

        private void AddDetailRow(StackPanel parent, string label, string value)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lbl = new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139))
            };

            var val = new TextBlock
            {
                Text = value ?? "—",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                TextAlignment = TextAlignment.Left
            };

            Grid.SetColumn(lbl, 0);
            Grid.SetColumn(val, 1);
            row.Children.Add(lbl);
            row.Children.Add(val);
            parent.Children.Add(row);

            parent.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(30, 51, 82)),
                Margin = new Thickness(0, 0, 0, 0),
                Opacity = 0.5
            });
        }

        private Button MakeActionButton(string text, Color bg)
        {
            var btn = new Button
            {
                Content = text,
                Height = 40,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(20, 0, 20, 0)
            };

            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, new SolidColorBrush(bg));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            factory.SetValue(Border.PaddingProperty, new Thickness(20, 0, 20, 0));
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AppendChild(cp);
            template.VisualTree = factory;
            btn.Template = template;

            return btn;
        }

        // ── Export as high-res PNG ───────────────────────────────────

        private void ExportAsImage(FrameworkElement element)
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Title = "حفظ الملف كصورة",
                    Filter = "PNG Image|*.png",
                    FileName = $"Student_{_student.StudentId}_{_student.FullName}.png"
                };

                if (dlg.ShowDialog() != true) return;

                element.UpdateLayout();

                double dpi = 300;
                double scale = dpi / 96d;

                int width = (int)(element.ActualWidth * scale);
                int height = (int)(element.ActualHeight * scale);

                var rtb = new RenderTargetBitmap(
                    width, height, dpi, dpi, PixelFormats.Pbgra32);

                var dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    dc.PushTransform(new ScaleTransform(
                        -1, 1,
                        element.ActualWidth / 2,
                        element.ActualHeight / 2));

                    var vb = new VisualBrush(element);
                    dc.DrawRectangle(vb, null,
                        new Rect(0, 0, element.ActualWidth, element.ActualHeight));

                    dc.Pop();
                }

                rtb.Render(dv);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using var fs = new FileStream(dlg.FileName, FileMode.Create);
                encoder.Save(fs);

                MessageBox.Show("تم حفظ الصورة بنجاح ✔");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ── Print ────────────────────────────────────────────────────

        private void PrintProfile()
        {
            try
            {
                var pd = new PrintDialog();
                if (pd.ShowDialog() != true) return;

                var doc = new System.Windows.Documents.FlowDocument
                {
                    PageWidth = pd.PrintableAreaWidth,
                    PageHeight = pd.PrintableAreaHeight,
                    FontSize = 13,
                    FlowDirection = FlowDirection.RightToLeft
                };

                var title = new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run($"ملف الطالب: {_student.FullName}"))
                {
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 12)
                };
                doc.Blocks.Add(title);

                void AddLine(string lbl, string val)
                {
                    var p = new System.Windows.Documents.Paragraph();
                    p.Inlines.Add(new System.Windows.Documents.Bold(
                        new System.Windows.Documents.Run(lbl + ":  ")));
                    p.Inlines.Add(new System.Windows.Documents.Run(val ?? "—"));
                    p.Margin = new Thickness(0, 3, 0, 3);
                    doc.Blocks.Add(p);
                }

                AddLine("رقم الهوية", _student.NationalId);
                AddLine("الصف الدراسي", _student.ClassName);
                AddLine("تاريخ الميلاد", _student.DateOfBirth.ToString("yyyy/MM/dd"));
                AddLine("الجنس", _student.Gender);
                AddLine("الهاتف", _student.Phone);
                AddLine("ولي الأمر", _student.GuardianName);
                AddLine("هوية ولي الأمر", _student.GuardianNationalId);
                AddLine("الصف السابق", _student.PreviousClass);
                AddLine("المعدل السابق", _student.PreviousGPA.ToString("F2"));
                AddLine("تاريخ التسجيل", _student.RegistrationDate.ToString("yyyy/MM/dd"));

                pd.PrintDocument(
                    ((System.Windows.Documents.IDocumentPaginatorSource)doc).DocumentPaginator,
                    $"طباعة ملف الطالب — {_student.FullName}");
            }
            catch (Exception ex) { MessageBox.Show("خطأ في الطباعة: " + ex.Message); }
        }
    }
}
