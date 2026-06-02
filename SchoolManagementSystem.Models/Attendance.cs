namespace SchoolManagementSystem.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }

        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } // Present / Absent
    }
}