namespace SchoolManagementSystem.BLL
{
    public class SchoolSummaryDto
    {
        public decimal SchoolAverage { get; set; }
        public decimal MaxScore { get; set; }
        public decimal MinScore { get; set; }
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSubjects { get; set; }
    }
}