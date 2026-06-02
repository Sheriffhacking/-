using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.BLL
{
    public class AttendanceService
    {
        private readonly AttendanceRepository repo = new AttendanceRepository();

        // ================= BASIC CRUD =================

        public void Add(Attendance a)
        {
            repo.Add(a);
        }

        public void Update(Attendance a)
        {
            repo.Update(a);
        }

        public void Delete(int id)
        {
            repo.Delete(id);
        }

        // ================= GET =================

        public List<Attendance> GetAll()
        {
            return repo.GetAll();
        }

        public List<Attendance> GetByDate(DateTime date)
        {
            return repo.GetByDate(date);
        }

        public List<Attendance> GetByStudent(int id)
        {
            return repo.GetByStudent(id);
        }

        // ================= SEARCH =================

        public List<Attendance> SearchByStudent(string name)
        {
            return repo.SearchByStudentName(name);
        }

        // ================= BUSINESS LOGIC (IMPORTANT) =================

        /// منع التكرار (نفس الطالب + نفس التاريخ)
        public bool Exists(int studentId, DateTime date)
        {
            return repo.Exists(studentId, date);
        }
        public (int حاضر, int غائب) GetStats(int studentId)
        {
            return repo.GetStudentAttendanceStats(studentId);
        }
        /// إدخال أو تحديث حسب وجود السجل
        public void AddOrUpdate(Attendance a)
        {
            if (Exists(a.StudentId, a.AttendanceDate))
            {
                repo.Update(a);
            }
            else
            {
                repo.Add(a);
            }
        }
    }
}