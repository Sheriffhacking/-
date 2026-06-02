using SchoolManagementSystem.DAL;
using System.Collections.Generic;

namespace SchoolManagementSystem.BLL
{
    public class AnalyticsService
    {
        private readonly AnalyticsRepository repo = new AnalyticsRepository();

        public List<dynamic> GetTopStudents() => repo.GetTopStudents();
        public List<dynamic> GetWeakStudents() => repo.GetWeakStudents();
        public List<dynamic> GetClassRanking() => repo.GetClassRanking();
        public List<dynamic> GetSubjectRanking() => repo.GetSubjectRanking();

        // 🔥 FIXED TYPE
        public AnalyticsRepository.SchoolSummaryDto GetSchoolSummary()
        {
            return repo.GetSchoolSummary();
        }

        public List<dynamic> GetClassPerformanceWithStats()
        {
            return repo.GetClassRanking();
        }
    }
}