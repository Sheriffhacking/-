// ============================================================
// TeacherTimetableViewModel.cs
// شاشة جدول المدرسين – قراءة فقط مع إحصائيات (FIXED)
// ============================================================

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SchoolManagementSystem.Repositories;

namespace SchoolManagementSystem.ViewModels
{
    // 🔥 مهم: توحيد Employee الحقيقي من Models
    using Employee = SchoolManagementSystem.Models.Employee;

    // خلية في جدول المدرس (للعرض فقط)
    public class TeacherCellVM : INotifyPropertyChanged
    {
        public int DayOrder { get; set; }
        public int PeriodNumber { get; set; }

        private string _text = "";
        private string _color = "#F5F7FA";

        public string DisplayText
        {
            get => _text;
            set { _text = value; OnPC(); OnPC(nameof(IsEmpty)); }
        }

        public string CardColor
        {
            get => _color;
            set { _color = value; OnPC(); }
        }

        public bool IsEmpty => string.IsNullOrEmpty(_text);

        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPC([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // إحصائيات الأيام
    public class DayStatItem
    {
        public string DayName { get; set; } = "";
        public int Count { get; set; }

        public double BarWidth => Math.Min(Count * 24, 168);
    }

    public class TeacherTimetableViewModel : INotifyPropertyChanged
    {
        public static readonly string[] DayNames =
            { "السبت", "الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس" };

        public static readonly int[] DayOrders = { 1, 2, 3, 4, 5, 6 };
        public const int PeriodCount = 6;

        private readonly TimetableRepository _repo = new();

        // ── lists ──
        public ObservableCollection<Employee> Employees { get; } = new();
        public ObservableCollection<TeacherCellVM> Cells { get; } = new();
        public ObservableCollection<DayStatItem> DayStats { get; } = new();

        // ── selection ──
        private Employee? _selectedEmployee;

        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPC();
                LoadTimetable();
            }
        }

        // ── totals ──
        private int _totalWeekly;
        public int TotalWeekly
        {
            get => _totalWeekly;
            set { _totalWeekly = value; OnPC(); }
        }

        // ── status ──
        private string _status = "اختر مدرساً لعرض جدوله";
        public string Status
        {
            get => _status;
            set { _status = value; OnPC(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPC(); }
        }

        // ── constructor ──
        public TeacherTimetableViewModel()
        {
            foreach (var d in DayOrders)
                for (int p = 1; p <= PeriodCount; p++)
                    Cells.Add(new TeacherCellVM
                    {
                        DayOrder = d,
                        PeriodNumber = p
                    });

            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                Employees.Clear();

                foreach (var e in _repo.GetEmployees())
                    Employees.Add(e);
            }
            catch (Exception ex)
            {
                Status = $"❌ {ex.Message}";
            }
        }

        private void LoadTimetable()
        {
            foreach (var c in Cells)
            {
                c.DisplayText = "";
                c.CardColor = "#F5F7FA";
            }

            DayStats.Clear();
            TotalWeekly = 0;

            if (_selectedEmployee == null)
                return;

            try
            {
                IsLoading = true;

                var entries = _repo.GetByTeacher(_selectedEmployee.EmployeeId);

                foreach (var entry in entries)
                {
                    var cell = Cells.FirstOrDefault(c =>
                        c.DayOrder == entry.DayOrder &&
                        c.PeriodNumber == entry.PeriodNumber);

                    if (cell == null) continue;

                    cell.DisplayText =
                        $"{entry.TeacherName}\n{entry.SubjectName}";

                    cell.CardColor = entry.SubjectColor;
                }

                for (int i = 0; i < DayOrders.Length; i++)
                {
                    int d = DayOrders[i];

                    DayStats.Add(new DayStatItem
                    {
                        DayName = DayNames[i],
                        Count = entries.Count(e => e.DayOrder == d)
                    });
                }

                TotalWeekly = entries.Count;

                Status =
                    $"✅ {_selectedEmployee.EmployeeName} – {TotalWeekly} حصة أسبوعياً";
            }
            catch (Exception ex)
            {
                Status = $"❌ {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPC([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}