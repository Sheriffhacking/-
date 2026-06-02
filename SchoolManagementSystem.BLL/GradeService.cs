using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System.Collections.Generic;

namespace SchoolManagementSystem.BLL
{
    public class GradeService
    {
        private readonly GradeRepository repo = new GradeRepository();

        // ================= ADD =================
        public void Add(Grade g)
        {
            repo.Add(g);
        }

        // ================= GET ALL WITH DETAILS =================
        public List<dynamic> GetAllGradesWithDetails(int classId)
        {
            return repo.GetAllGradesWithDetails(classId);
        }

        // ================= GET BY STUDENT =================
        public List<Grade> GetByStudent(int id)
        {
            return repo.GetByStudent(id);
        }

        // ================= CLASS + SUBJECT =================
        public List<Grade> GetClassSubjectGrades(int classId, int subjectId)
        {
            return repo.GetClassSubjectGrades(classId, subjectId);
        }

        // ================= DELETE (FIXED PROPERLY) =================
        public void Delete(int gradeId)
        {
            repo.Delete(gradeId);
        }
        public List<Grade> GetByClass(int classId)
        {
            return repo.GetByClass(classId);
        }

        // ================= EXPORT / REPORT =================
        public List<dynamic> GetGradesByClassAndSubject(int classId, int subjectId)
        {
            return repo.GetGradesByClassAndSubject(classId, subjectId);
        }
    }
}