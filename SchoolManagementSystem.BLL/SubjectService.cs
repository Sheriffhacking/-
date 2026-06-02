using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System.Collections.Generic;

namespace SchoolManagementSystem.BLL
{
    public class SubjectService
    {
        private readonly SubjectRepository repo =
            new SubjectRepository();

        public List<Subject> GetAllSubjects()
        {
            return repo.GetAll();
        }
    }
}