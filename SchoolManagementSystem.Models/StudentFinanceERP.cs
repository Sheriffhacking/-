namespace SchoolManagementSystem.Models
{
    public class StudentFinanceERP
    {
        public int StudentId { get; set; }

        public string StudentName { get; set; }

        public int ClassId { get; set; }
        public string ClassName { get; set; }

        public int FeeTypeId { get; set; }
        public string FeeName { get; set; }

        public string AcademicYear { get; set; }

        public int StudyMonth { get; set; }

        public decimal RequiredAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public string PaymentStatus { get; set; }
    }
}