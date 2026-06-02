namespace SchoolManagementSystem.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
      
        public decimal MaxMark { get; set; }
        public bool IsActive { get; set; }

        public override string ToString() => SubjectName;
        public string ColorHex { get; set; } = "#E0E0E0";
    }
}
