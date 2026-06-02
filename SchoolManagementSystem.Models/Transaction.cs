namespace SchoolManagementSystem.Models
{
    public class Transaction
    {
        public decimal TotalRequiredAmount { get; set; }

        public decimal RemainingAmount { get; set; }
        public int? FeeTypeId { get; set; }

        public string AcademicYear { get; set; }

        public int? StudyMonth { get; set; }

        public string PaymentMethod { get; set; }

        public string FeeName { get; set; }
        public int TransactionId { get; set; }
        public string Type { get; set; }
        public int? StudentId { get; set; }
        public int? EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        public string StudentName { get; set; }
        public string EmployeeName { get; set; }
    }
}