using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System.Collections.Generic;

namespace SchoolManagementSystem.BLL
{
    public class ClassService
    {
        private readonly ClassRepository repo = new ClassRepository();

        // =========================
        // OLD COMPATIBILITY (UI FIX)
        // =========================
        public List<Class> GetAllClasses()
        {
            return repo.GetAllClasses();
        }

        public void AddClass(Class cls)
        {
            repo.Add(cls);
        }

        public void UpdateClass(Class cls)
        {
            repo.Update(cls);
        }

        public void DeleteClass(int id)
        {
            repo.Delete(id);
        }

        // =========================
        // ERP STANDARD
        // =========================
        public List<Class> GetAll()
        {
            return repo.GetAllClasses();
        }

        public void Add(Class cls)
        {
            repo.Add(cls);
        }

        public void Update(Class cls)
        {
            repo.Update(cls);
        }

        public void Delete(int id)
        {
            repo.Delete(id);
        }
    }
}