public class StudentStatement
{
    public string StudentName { get; set; }
    public string FeeName { get; set; }
    public string AcademicYear { get; set; }
    public int StudyMonth { get; set; }

    public decimal RequiredAmount { get; set; }
    public decimal PaidAmount { get; set; }

    public decimal Remaining => RequiredAmount - PaidAmount;

    public string Status { get; set; }
}