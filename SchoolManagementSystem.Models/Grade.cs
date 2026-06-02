using System;

namespace SchoolManagementSystem.Models
{
    public class Grade
    {
        public int GradeId { get; set; }

        public int StudentId { get; set; }

        public string StudentName { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; }

        public string ExamType { get; set; }

        public string Semester { get; set; }

        public decimal Score { get; set; }

        public string Notes { get; set; }
        public string ClassName { get; set; }

        public DateTime GradeDate { get; set; }

        // التقدير التلقائي
        public string Evaluation
        {
            get
            {
                if (Score >= 90)
                    return "ممتاز";

                if (Score >= 80)
                    return "جيد جداً";

                if (Score >= 70)
                    return "جيد";

                if (Score >= 60)
                    return "مقبول";
                if (Score >= 50)
                    return "ضعيف جدا ";
                return "راسب";
            }
        }
    }
}