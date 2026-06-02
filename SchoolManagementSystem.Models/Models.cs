// ============================================================
// Models.cs  –  4 نماذج فقط بدون أي تعقيد
// ============================================================

namespace SchoolManagementSystem.Models.Models
{
    public class Class
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = "";
        public override string ToString() => ClassName;
    }

    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = "";
        public string ColorHex { get; set; } = "#E0E0E0";
        public override string ToString() => SubjectName;
    }

    public class Employee
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public override string ToString() => EmployeeName;
    }

    public class ClassTimetableEntry
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int? SubjectId { get; set; }
        public int? TeacherId { get; set; }
        public int DayOrder { get; set; }
        public int PeriodNumber { get; set; }
        // navigation
        public string SubjectName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public string SubjectColor { get; set; } = "#E0E0E0";
    }
}
