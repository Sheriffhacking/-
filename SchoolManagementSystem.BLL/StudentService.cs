using SchoolManagementSystem.DAL;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.BLL
{
    public class StudentService
    {
        private readonly StudentRepository repo = new StudentRepository();

        // =========================
        // ➕ إضافة طالب
        // يعيد الـ ID الجديد عند النجاح، أو -1 عند الفشل
        // =========================
        public int AddStudent(Student student)
        {
            if (string.IsNullOrWhiteSpace(student.FullName))
                throw new Exception("اسم الطالب مطلوب");

            if (string.IsNullOrWhiteSpace(student.NationalId))
                throw new Exception("رقم الهوية مطلوب");

            if (student.ClassId <= 0)
                throw new Exception("يجب اختيار الصف الدراسي");

            if (student.PreviousGPA < 0 || student.PreviousGPA > 100)
                throw new Exception("المعدل يجب أن يكون بين 0 و 100");

            try
            {
                return repo.AddStudent(student); // يعيد الـ ID الجديد
            }
            catch (Exception ex)
            {
                throw new Exception("حدث خطأ أثناء الحفظ: " + ex.Message);
            }
        }

        // =========================
        // 📥 جلب جميع الطلاب
        // =========================
        public List<Student> GetAllStudents()
        {
            try
            {
                var list = repo.GetAllStudents();

                foreach (var s in list)
                    s.ClassName ??= "غير محدد";

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("خطأ أثناء تحميل الطلاب: " + ex.Message);
            }
        }

        // =========================
        // 📚 جلب الطلاب حسب الصف
        // =========================
        public List<Student> GetStudentsByClass(int classId)
        {
            try
            {
                var list = repo.GetStudentsByClass(classId);

                foreach (var s in list)
                    s.ClassName ??= "غير محدد";

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("خطأ أثناء تحميل طلاب الصف: " + ex.Message);
            }
        }

        // =========================
        // ✏️ تعديل طالب
        // =========================
        public string UpdateStudent(Student student)
        {
            if (string.IsNullOrWhiteSpace(student.FullName))
                return "اسم الطالب مطلوب";

            if (string.IsNullOrWhiteSpace(student.NationalId))
                return "رقم الهوية مطلوب";

            if (student.ClassId <= 0)
                return "يجب اختيار الصف الدراسي";

            if (student.PreviousGPA < 0 || student.PreviousGPA > 100)
                return "المعدل يجب أن يكون بين 0 و 100";

            try
            {
                repo.UpdateStudent(student);
                return "تم تعديل بيانات الطالب بنجاح";
            }
            catch (Exception ex)
            {
                return "خطأ أثناء التعديل: " + ex.Message;
            }
        }

        // =========================
        // 🗑️ حذف طالب
        // =========================
        public string DeleteStudent(int studentId)
        {
            if (studentId <= 0)
                return "رقم الطالب غير صالح";

            try
            {
                repo.DeleteStudent(studentId);
                return "تم حذف الطالب بنجاح";
            }
            catch (Exception ex)
            {
                return "خطأ أثناء الحذف: " + ex.Message;
            }
        }

        // =========================
        // 📎 إضافة مرفق للطالب
        // =========================
        public void AddAttachment(StudentAttachment attachment)
        {
            if (attachment == null)
                throw new Exception("بيانات المرفق غير صالحة");

            if (attachment.StudentId <= 0)
                throw new Exception("رقم الطالب غير صالح");

            if (attachment.FileData == null || attachment.FileData.Length == 0)
                throw new Exception("الملف المرفق فارغ");

            if (string.IsNullOrWhiteSpace(attachment.FileName))
                throw new Exception("اسم الملف مطلوب");

            try
            {
                repo.AddAttachment(attachment);
            }
            catch (Exception ex)
            {
                throw new Exception("خطأ أثناء حفظ المرفق: " + ex.Message);
            }
        }

        // =========================
        // 📂 جلب مرفقات طالب
        // =========================
        public List<StudentAttachment> GetAttachments(int studentId)
        {
            if (studentId <= 0)
                throw new Exception("رقم الطالب غير صالح");

            try
            {
                return repo.GetAttachments(studentId);
            }
            catch (Exception ex)
            {
                throw new Exception("خطأ أثناء جلب المرفقات: " + ex.Message);
            }
        }

        // =========================
        // 🔢 عدد مرفقات الطالب
        // =========================
        public int GetAttachmentCount(int studentId)
        {
            if (studentId <= 0) return 0;

            try
            {
                return repo.GetAttachmentCount(studentId);
            }
            catch
            {
                return 0;
            }
        }

        // =========================
        // 🗑️ حذف مرفق
        // =========================
        public void DeleteAttachment(int attachmentId)
        {
            if (attachmentId <= 0)
                throw new Exception("رقم المرفق غير صالح");

            try
            {
                repo.DeleteAttachment(attachmentId);
            }
            catch (Exception ex)
            {
                throw new Exception("خطأ أثناء حذف المرفق: " + ex.Message);
            }
        }
    }
}