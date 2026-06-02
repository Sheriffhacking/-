// ============================================================
// ClassTimetableViewModel.cs - FINAL FIXED VERSION
// ============================================================

using SchoolManagementSystem.Repositories;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SchoolManagementSystem.UI.ViewModels
{
    // ================================================================
    // ClassTimetableEntry
    // ================================================================
    public class ClassTimetableEntry
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int? SubjectId { get; set; }
        public int? TeacherId { get; set; }
        public int DayOrder { get; set; }
        public int PeriodNumber { get; set; }
        public string SubjectName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public string SubjectColor { get; set; } = "#E0E0E0";
    }

    // ================================================================
    // CellVM
    // ================================================================
    public class CellVM : INotifyPropertyChanged
    {
        public int ClassId { get; init; }
        public int DayOrder { get; init; }
        public int PeriodNumber { get; init; }

        private Subject? _subject;
        private SchoolManagementSystem.Models.Employee? _teacher;
        private bool _isSaving;

        public Subject? SelectedSubject
        {
            get => _subject;
            set
            {
                _subject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CardColor));
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public SchoolManagementSystem.Models.Employee? SelectedTeacher
        {
            get => _teacher;
            set
            {
                _teacher = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set { _isSaving = value; OnPropertyChanged(); }
        }

        public string CardColor => _subject?.ColorHex ?? "#F5F7FA";
        public bool IsEmpty => _subject == null && _teacher == null;

        public Action<CellVM>? OnChanged { get; set; }
        public void NotifyChanged() => OnChanged?.Invoke(this);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // ================================================================
    // StatItem
    // ================================================================
    public class StatItem : INotifyPropertyChanged
    {
        private string _label = "";
        private int _count;
        private string _color = "#E0E0E0";

        public string Label
        {
            get => _label;
            set { _label = value; OnPC(); }
        }

        public int Count
        {
            get => _count;
            set { _count = value; OnPC(); OnPC(nameof(BarWidth)); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPC(); }
        }

        public double BarWidth => Math.Min(Count * 15, 200);

        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPC([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // ================================================================
    // ViewModel
    // ================================================================
    public class ClassTimetableViewModel : INotifyPropertyChanged
    {
        public static readonly string[] DayNames =
            { "السبت", "الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس" };

        public static readonly int[] DayOrders = { 1, 2, 3, 4, 5, 6 };
        public const int PeriodCount = 6;

        private readonly TimetableRepository _repo = new();

        public ObservableCollection<Class> Classes { get; } = new();
        public ObservableCollection<Subject> Subjects { get; } = new();

        // ✅ FIX: توحيد النوع بالكامل
        public ObservableCollection<SchoolManagementSystem.Models.Employee> Employees { get; } = new();

        public Dictionary<(int D, int P), CellVM> Cells { get; } = new();
        public ObservableCollection<StatItem> SubjectStats { get; } = new();

        private Class? _selectedClass;
        public Class? SelectedClass
        {
            get => _selectedClass;
            set { _selectedClass = value; OnPC(); LoadTimetable(); }
        }

        private string _status = "اختر صفاً لتحميل الجدول";
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

        public ClassTimetableViewModel()
        {
            foreach (var d in DayOrders)
                for (int p = 1; p <= PeriodCount; p++)
                {
                    var cell = new CellVM { DayOrder = d, PeriodNumber = p };
                    cell.OnChanged = OnCellChanged;
                    Cells[(d, p)] = cell;
                }

            LoadLookups();
        }

        private void LoadLookups()
        {
            foreach (var c in _repo.GetClasses()) Classes.Add(c);
            foreach (var s in _repo.GetSubjects()) Subjects.Add(s);
            foreach (var e in _repo.GetEmployees()) Employees.Add(e);
        }

        private void LoadTimetable()
        {
            foreach (var cell in Cells.Values)
            {
                cell.SelectedSubject = null;
                cell.SelectedTeacher = null;
            }

            if (_selectedClass == null) return;

            IsLoading = true;

            var entries = _repo.GetByClass(_selectedClass.ClassId);

            foreach (var entry in entries)
            {
                if (!Cells.TryGetValue((entry.DayOrder, entry.PeriodNumber), out var cell))
                    continue;

                cell.SelectedSubject = entry.SubjectId.HasValue
                    ? Subjects.FirstOrDefault(s => s.SubjectId == entry.SubjectId.Value)
                    : null;

                cell.SelectedTeacher = entry.TeacherId.HasValue
                    ? Employees.FirstOrDefault(e => e.EmployeeId == entry.TeacherId.Value)
                    : null;
            }

            Status = $"تم تحميل جدول {_selectedClass.ClassName}";
            RefreshStats();
            IsLoading = false;
        }

        private void OnCellChanged(CellVM cell)
        {
            if (_selectedClass == null) return;

            cell.IsSaving = true;

            _repo.UpsertCell(
                _selectedClass.ClassId,
                cell.DayOrder,
                cell.PeriodNumber,
                cell.SelectedSubject?.SubjectId,
                cell.SelectedTeacher?.EmployeeId
            );

            RefreshStats();
            Status = "تم الحفظ";
            cell.IsSaving = false;
        }

        private void RefreshStats()
        {
            SubjectStats.Clear();

            var grouped = Cells.Values
                .Where(c => c.SelectedSubject != null)
                .GroupBy(c => c.SelectedSubject!)
                .OrderByDescending(g => g.Count());

            foreach (var g in grouped)
            {
                SubjectStats.Add(new StatItem
                {
                    Label = g.Key.SubjectName,
                    Count = g.Count(),
                    Color = g.Key.ColorHex
                });
            }
        }
        public void ClearAll()
        {
            if (SelectedClass == null) return;

            // مسح من الذاكرة (UI)
            foreach (var cell in Cells.Values)
            {
                cell.SelectedSubject = null;
                cell.SelectedTeacher = null;
            }

            // مسح من قاعدة البيانات (مهم جداً)
            _repo.DeleteByClassId(SelectedClass.ClassId);

            // تحديث الإحصائيات
            SubjectStats.Clear();

            Status = $"تم مسح جدول {SelectedClass.ClassName}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPC([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}