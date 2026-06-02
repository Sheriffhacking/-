namespace SchoolManagementSystem.Models
{
    public class Student
    {
        public DateTime RegistrationDate { get; set; }
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string NationalId { get; set; }

        public int ClassId { get; set; }   // 🔥 مهم للعلاقة

        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }

        public string GuardianName { get; set; }
        public string GuardianNationalId { get; set; }

        public string PreviousClass { get; set; }
        public decimal PreviousGPA { get; set; }
        public string ClassName { get; set; }
        public bool IsActive { get; set; }
        // في كلاس Student
        public byte[] PhotoData { get; set; }        // صورة الطالب
     
    }
}