// ============================================================
// TeacherTimetablePage.xaml.cs
// ============================================================

using SchoolManagementSystem.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SchoolManagementSystem.Views;

    

namespace SchoolManagementSystem.Views
{
    public partial class TeacherTimetablePage : Page
    {
        public TeacherTimetableViewModel VM { get; }

        public TeacherTimetablePage()
        {
            InitializeComponent();
            VM = new TeacherTimetableViewModel();
            DataContext = VM;

            BuildGrid();

            // أعد بناء الخلايا عند تغيير البيانات
            VM.Cells.CollectionChanged += (_, _) => RefreshCells();
            VM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TeacherTimetableViewModel.IsLoading) && !VM.IsLoading)
                    RefreshCells();
            };
        }

        // ============================================================
        // بناء هيكل الجدول (مرة واحدة)
        // ============================================================
        private void BuildGrid()
        {
            var g = TeacherGrid;
            g.RowDefinitions.Clear();
            g.ColumnDefinitions.Clear();
            g.Children.Clear();

            int days = TeacherTimetableViewModel.DayOrders.Length;
            int periods = TeacherTimetableViewModel.PeriodCount;

            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            for (int d = 0; d < days; d++)
                g.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(1, GridUnitType.Star), MinWidth = 140 });

            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(44) });
            for (int p = 0; p < periods; p++)
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(76) });

            // corner
            AddToGrid(g, HeaderCell("الحصة / اليوم", Color.FromRgb(30, 58, 95)), 0, 0);

            // day headers
            for (int d = 0; d < days; d++)
            {
                var h = HeaderCell(
                    TeacherTimetableViewModel.DayNames[d],
                    Color.FromRgb(124, 58, 237));
                AddToGrid(g, h, 0, d + 1);
            }

            // period headers + data cells
            for (int p = 0; p < periods; p++)
            {
                AddToGrid(g, PeriodHeader(p + 1), p + 1, 0);

                for (int d = 0; d < days; d++)
                {
                    var b = MakeDataCell(
                        TeacherTimetableViewModel.DayOrders[d],
                        p + 1,
                        p % 2 == 0);
                    AddToGrid(g, b, p + 1, d + 1);
                }
            }
        }

        // ============================================================
        // تحديث محتوى الخلايا بعد تحميل البيانات
        // ============================================================
        private void RefreshCells()
        {
            WelcomePanel.Visibility = VM.SelectedEmployee == null
                ? Visibility.Visible : Visibility.Collapsed;

            // ابحث عن كل Border بـ Tag وحدّث محتواه
            foreach (var el in TeacherGrid.Children)
            {
                if (el is Border b && b.Tag is (int day, int period))
                {
                    var cell = VM.Cells.FirstOrDefault(
                        c => c.DayOrder == day && c.PeriodNumber == period);
                    UpdateDataCell(b, cell);
                }
            }
        }

        // ============================================================
        // صنع خلية بيانات (للعرض فقط)
        // ============================================================
        private Border MakeDataCell(int dayOrder, int periodNumber, bool even)
        {
            var b = new Border
            {
                Background = new SolidColorBrush(even
                    ? Color.FromRgb(249, 250, 251)
                    : Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Tag = (dayOrder, periodNumber),
                Padding = new Thickness(8, 6, 8, 6)
            };

            var cell = VM.Cells.FirstOrDefault(
                c => c.DayOrder == dayOrder && c.PeriodNumber == periodNumber);
            UpdateDataCell(b, cell);
            return b;
        }

        private static void UpdateDataCell(Border b, TeacherCellVM? cell)
        {
            if (cell == null || cell.IsEmpty)
            {
                b.Child = new TextBlock
                {
                    Text = "—",
                    Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                b.Background = new SolidColorBrush(
                    b.Tag is (int, int) ? Color.FromRgb(249, 250, 251) : Color.FromRgb(255, 255, 255));
                return;
            }

            // لون خفيف
            try
            {
                var hex = cell.CardColor.TrimStart('#');
                if (hex.Length == 6)
                    b.Background = new SolidColorBrush(Color.FromArgb(
                        45,
                        Convert.ToByte(hex[..2], 16),
                        Convert.ToByte(hex[2..4], 16),
                        Convert.ToByte(hex[4..6], 16)));
            }
            catch { }

            var lines = cell.DisplayText.Split('\n');
            var panel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            if (lines.Length >= 1)
                panel.Children.Add(new TextBlock
                {
                    Text = lines[0],
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });

            if (lines.Length >= 2)
                panel.Children.Add(new TextBlock
                {
                    Text = lines[1],
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0)
                });

            b.Child = panel;
        }

        // ============================================================
        // Helpers
        // ============================================================
        private static Border HeaderCell(string text, Color bg) =>
            new()
            {
                Background = new SolidColorBrush(bg),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

        private static Border PeriodHeader(int period) =>
            new()
            {
                Background = new SolidColorBrush(Color.FromRgb(245, 243, 255)),
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
                            Text = $"ح{period}", FontSize = 13,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush(Color.FromRgb(124, 58, 237)),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = $"الحصة {period}", FontSize = 9,
                            Foreground = new SolidColorBrush(Color.FromRgb(167, 139, 250)),
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                }
            };

        private static void AddToGrid(Grid g, UIElement el, int row, int col)
        {
            Grid.SetRow(el, row);
            Grid.SetColumn(el, col);
            g.Children.Add(el);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
                pd.PrintVisual(TeacherGrid, $"جدول – {VM.SelectedEmployee?.EmployeeName}");
        }
    }
}
