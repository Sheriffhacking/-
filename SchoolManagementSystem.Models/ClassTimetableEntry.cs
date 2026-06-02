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