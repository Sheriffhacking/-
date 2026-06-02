// ============================================================
// ClassTimetablePage.xaml.cs
// بناء الجدول ديناميكياً + ربط ComboBoxes + أزرار
// ============================================================

using SchoolManagementSystem.UI.ViewModels;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SchoolManagementSystem.UI;

namespace SchoolManagementSystem.Views
{
    public partial class ClassTimetablePage : Page
    {
        public ClassTimetableViewModel VM { get; }

        // ألوان التمييز بين الصفوف الزوجية والفردية
        private static readonly SolidColorBrush RowBg1 = new(Color.FromRgb(249, 250, 251));
        private static readonly SolidColorBrush RowBg2 = new(Color.FromRgb(255, 255, 255));

        public ClassTimetablePage()
        {
            InitializeComponent();
            VM = new ClassTimetableViewModel();
            DataContext = VM;

            BuildGrid();
            WelcomePanel.Visibility = Visibility.Visible;
        }

        // ============================================================
        // بناء الجدول الديناميكي مرة واحدة عند فتح الصفحة
        // ============================================================
        private void BuildGrid()
        {
            var grid = TimetableGrid;
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            int days = ClassTimetableViewModel.DayOrders.Length;   // 6
            int periods = ClassTimetableViewModel.PeriodCount;         // 6

            // ── تعريف الأعمدة: عمود Header + 6 أيام ──
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            for (int d = 0; d < days; d++)
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(1, GridUnitType.Star), MinWidth = 155 });

            // ── تعريف الصفوف: صف Header + 6 حصص ──
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(44) });
            for (int p = 0; p < periods; p++)
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(90) });

            // ── Header الزاوية ──
            AddCell(grid, MakeCornerHeader("الحصة / اليوم"), 0, 0);

            // ── Headers الأيام ──
            for (int d = 0; d < days; d++)
                AddCell(grid, MakeDayHeader(ClassTimetableViewModel.DayNames[d]), 0, d + 1);

            // ── الحصص ──
            for (int p = 0; p < periods; p++)
            {
                // header الحصة
                AddCell(grid, MakePeriodHeader(p + 1), p + 1, 0);

                // خلايا الأيام
                for (int d = 0; d < days; d++)
                {
                    int dayOrder = ClassTimetableViewModel.DayOrders[d];
                    int periodNumber = p + 1;
                    var bg = p % 2 == 0 ? RowBg1 : RowBg2;

                    var cellUI = MakeCellUI(dayOrder, periodNumber, bg);
                    AddCell(grid, cellUI, p + 1, d + 1);
                }
            }
        }

        // ============================================================
        // خلية المادة + المدرس (ComboBox × ComboBox)
        // ============================================================
        private UIElement MakeCellUI(int dayOrder, int periodNumber, SolidColorBrush bg)
        {
            var cell = VM.Cells[(dayOrder, periodNumber)];

            var outer = new Border
            {
                Background = bg,
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(6, 4, 6, 4)
            };

            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            // ── ComboBox المادة ──
            var cmbSubject = new ComboBox
            {
                ItemsSource = VM.Subjects,
                DisplayMemberPath = "SubjectName",
                FontSize = 12,
                Height = 30,
                Margin = new Thickness(0, 0, 0, 4),
                ToolTip = "اختر المادة",
                Tag = cell
            };

            // Placeholder via style
            cmbSubject.Resources.Add(SystemColors.WindowBrushKey,
                new SolidColorBrush(Color.FromRgb(248, 250, 252)));

            // ربط SelectedItem بالـ cell
            cmbSubject.SelectedItem = cell.SelectedSubject;
            cmbSubject.SelectionChanged += (s, e) =>
            {
                var c = (CellVM)((ComboBox)s).Tag;
                c.SelectedSubject = (Models.Subject?)cmbSubject.SelectedItem;

                // لون الخلية حسب المادة
                outer.Background = c.SelectedSubject != null
                    ? MakeLightBrush(c.SelectedSubject.ColorHex)
                    : bg;

                c.NotifyChanged();
            };

            // ── ComboBox المدرس ──
            var cmbTeacher = new ComboBox
            {
                ItemsSource = VM.Employees,
                DisplayMemberPath = "EmployeeName",
                FontSize = 11,
                Height = 28,
                ToolTip = "اختر المدرس",
                Tag = cell
            };
            cmbTeacher.SelectedItem = cell.SelectedTeacher;
            cmbTeacher.SelectionChanged += (s, e) =>
            {
                var c = (CellVM)((ComboBox)s).Tag;
                c.SelectedTeacher = (Models.Employee?)cmbTeacher.SelectedItem;
                c.NotifyChanged();
            };

            // تحديث الـ UI عند تغيير البيانات (مثلاً عند تحميل صف جديد)
            cell.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CellVM.SelectedSubject))
                {
                    cmbSubject.SelectedItem = cell.SelectedSubject;
                    outer.Background = cell.SelectedSubject != null
                        ? MakeLightBrush(cell.SelectedSubject.ColorHex)
                        : bg;
                }
                if (e.PropertyName == nameof(CellVM.SelectedTeacher))
                    cmbTeacher.SelectedItem = cell.SelectedTeacher;
            };

            stack.Children.Add(cmbSubject);
            stack.Children.Add(cmbTeacher);
            outer.Child = stack;
            return outer;
        }

        // ============================================================
        // Helpers: صنع عناصر الـ Header
        // ============================================================
        private static UIElement MakeCornerHeader(string text)
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        private static UIElement MakeDayHeader(string dayName)
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(37, 99, 235)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(29, 78, 216)),
                BorderThickness = new Thickness(1, 0, 0, 0),
                Child = new TextBlock
                {
                    Text = dayName,
                    Foreground = Brushes.White,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        private static UIElement MakePeriodHeader(int period)
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Child = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"ح{period}",
                            FontSize = 13, FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = $"الحصة {period}",
                            FontSize = 9,
                            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                }
            };
        }

        private static void AddCell(Grid grid, UIElement el, int row, int col)
        {
            Grid.SetRow(el, row);
            Grid.SetColumn(el, col);
            grid.Children.Add(el);
        }

        // لون فاتح من HEX (opacity 25%)
        private static SolidColorBrush MakeLightBrush(string hex)
        {
            try
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                {
                    byte r = Convert.ToByte(hex[..2], 16);
                    byte g = Convert.ToByte(hex[2..4], 16);
                    byte b = Convert.ToByte(hex[4..6], 16);
                    return new SolidColorBrush(Color.FromArgb(50, r, g, b));
                }
            }
            catch { }
            return new SolidColorBrush(Color.FromRgb(248, 250, 252));
        }

        // ============================================================
        // EVENT HANDLERS
        // ============================================================
        private void ClassCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WelcomePanel.Visibility = VM.SelectedClass == null
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
                pd.PrintVisual(TimetableGrid, $"جدول – {VM.SelectedClass?.ClassName}");
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (VM.SelectedClass == null)
            {
                MessageBox.Show("اختر صفاً أولاً.", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV|*.csv",
                    FileName = $"جدول_{VM.SelectedClass.ClassName}.csv"
                };
                if (dlg.ShowDialog() != true) return;

                var sb = new StringBuilder("اليوم,الحصة,المادة,المدرس\n");
                foreach (var di in Enumerable.Range(0, ClassTimetableViewModel.DayNames.Length))
                {
                    int d = ClassTimetableViewModel.DayOrders[di];
                    string dayName = ClassTimetableViewModel.DayNames[di];
                    for (int p = 1; p <= ClassTimetableViewModel.PeriodCount; p++)
                    {
                        var cell = VM.Cells[(d, p)];
                        if (!cell.IsEmpty)
                            sb.AppendLine($"{dayName},{p},{cell.SelectedSubject?.SubjectName},{cell.SelectedTeacher?.EmployeeName}");
                    }
                }
                System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("✅ تم التصدير بنجاح.", "تصدير",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (VM.SelectedClass == null) return;
            var r = MessageBox.Show(
                $"هل تريد مسح جدول {VM.SelectedClass.ClassName} بالكامل؟",
                "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes) VM.ClearAll();
        }
    }
}
