using Microsoft.Data.SqlClient;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SchoolManagementSystem.DAL
{
    public class StudentRepository
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        // ================= ADD (يعيد ID الجديد) =================
        public int AddStudent(Student student)
        {
            using SqlConnection conn = db.GetConnection();

            string query = @"
            INSERT INTO Students 
            (
                FullName,
                NationalId,
                ClassId,
                DateOfBirth,
                RegistrationDate,
                Gender,
                Phone,
                GuardianName,
                GuardianNationalId,
                PreviousClass,
                PreviousGPA,
                IsActive,
                PhotoData
            )
            VALUES
            (
                @FullName,
                @NationalId,
                @ClassId,
                @DateOfBirth,
                @RegistrationDate,
                @Gender,
                @Phone,
                @GuardianName,
                @GuardianNationalId,
                @PreviousClass,
                @PreviousGPA,
                @IsActive,
                @PhotoData
            );
            SELECT SCOPE_IDENTITY();";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@FullName", student.FullName);
            cmd.Parameters.AddWithValue("@NationalId", student.NationalId);
            cmd.Parameters.AddWithValue("@ClassId", student.ClassId);
            cmd.Parameters.AddWithValue("@DateOfBirth", student.DateOfBirth);

            cmd.Parameters.AddWithValue("@RegistrationDate",
                student.RegistrationDate == DateTime.MinValue
                    ? DateTime.Now
                    : student.RegistrationDate);

            cmd.Parameters.AddWithValue("@Gender", student.Gender ?? "");
            cmd.Parameters.AddWithValue("@Phone", student.Phone ?? "");
            cmd.Parameters.AddWithValue("@GuardianName", student.GuardianName ?? "");
            cmd.Parameters.AddWithValue("@GuardianNationalId", student.GuardianNationalId ?? "");
            cmd.Parameters.AddWithValue("@PreviousClass", student.PreviousClass ?? "");
            cmd.Parameters.AddWithValue("@PreviousGPA", student.PreviousGPA);
            cmd.Parameters.AddWithValue("@IsActive", student.IsActive);

            // الصورة: إذا لم توجد نمرر DBNull
            if (student.PhotoData != null && student.PhotoData.Length > 0)
                cmd.Parameters.AddWithValue("@PhotoData", student.PhotoData);
            else
                cmd.Parameters.AddWithValue("@PhotoData", DBNull.Value);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        // ================= GET ALL (WITH CLASS NAME + PHOTO) =================
        public List<Student> GetAllStudents()
        {
            List<Student> list = new();

            using SqlConnection conn = db.GetConnection();

            string query = @"
            SELECT 
                s.*,
                c.ClassName
            FROM Students s
            INNER JOIN Classes c ON s.ClassId = c.ClassId";

            using SqlCommand cmd = new SqlCommand(query, conn);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Student
                {
                    StudentId = (int)reader["StudentId"],
                    FullName = reader["FullName"].ToString(),
                    NationalId = reader["NationalId"].ToString(),
                    ClassId = (int)reader["ClassId"],
                    ClassName = reader["ClassName"].ToString(),
                    DateOfBirth = (DateTime)reader["DateOfBirth"],
                    RegistrationDate = reader["RegistrationDate"] == DBNull.Value
                                            ? DateTime.Now
                                            : (DateTime)reader["RegistrationDate"],
                    Gender = reader["Gender"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    GuardianName = reader["GuardianName"].ToString(),
                    GuardianNationalId = reader["GuardianNationalId"].ToString(),
                    PreviousClass = reader["PreviousClass"].ToString(),
                    PreviousGPA = reader["PreviousGPA"] == DBNull.Value
                                            ? 0
                                            : (decimal)reader["PreviousGPA"],
                    IsActive = reader["IsActive"] != DBNull.Value && (bool)reader["IsActive"],
                    PhotoData = reader["PhotoData"] == DBNull.Value
                                            ? null
                                            : (byte[])reader["PhotoData"]
                });
            }

            return list;
        }

        // ================= GET BY CLASS =================
        public List<Student> GetStudentsByClass(int classId)
        {
            var list = new List<Student>();

            using SqlConnection conn = db.GetConnection();

            string query = @"
            SELECT 
                s.StudentId,
                s.FullName,
                s.NationalId,
                s.ClassId,
                c.ClassName
            FROM Students s
            INNER JOIN Classes c ON s.ClassId = c.ClassId
            WHERE s.ClassId = @classId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@classId", classId);

            conn.Open();

            using SqlDataReader r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Student
                {
                    StudentId = (int)r["StudentId"],
                    FullName = r["FullName"].ToString(),
                    NationalId = r["NationalId"].ToString(),
                    ClassId = (int)r["ClassId"],
                    ClassName = r["ClassName"].ToString()
                });
            }

            return list;
        }

        // ================= UPDATE (مع الصورة) =================
        public void UpdateStudent(Student student)
        {
            using SqlConnection conn = db.GetConnection();

            string query = @"
            UPDATE Students SET
                FullName           = @FullName,
                NationalId         = @NationalId,
                ClassId            = @ClassId,
                DateOfBirth        = @DateOfBirth,
                Gender             = @Gender,
                Phone              = @Phone,
                GuardianName       = @GuardianName,
                GuardianNationalId = @GuardianNationalId,
                PreviousClass      = @PreviousClass,
                PreviousGPA        = @PreviousGPA,
                IsActive           = @IsActive,
                PhotoData          = @PhotoData
            WHERE StudentId = @StudentId";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@FullName", student.FullName);
            cmd.Parameters.AddWithValue("@NationalId", student.NationalId);
            cmd.Parameters.AddWithValue("@ClassId", student.ClassId);
            cmd.Parameters.AddWithValue("@DateOfBirth", student.DateOfBirth);
            cmd.Parameters.AddWithValue("@Gender", student.Gender ?? "");
            cmd.Parameters.AddWithValue("@Phone", student.Phone ?? "");
            cmd.Parameters.AddWithValue("@GuardianName", student.GuardianName ?? "");
            cmd.Parameters.AddWithValue("@GuardianNationalId", student.GuardianNationalId ?? "");
            cmd.Parameters.AddWithValue("@PreviousClass", student.PreviousClass ?? "");
            cmd.Parameters.AddWithValue("@PreviousGPA", student.PreviousGPA);
            cmd.Parameters.AddWithValue("@IsActive", student.IsActive);
            cmd.Parameters.AddWithValue("@StudentId", student.StudentId);

            if (student.PhotoData != null && student.PhotoData.Length > 0)
                cmd.Parameters.AddWithValue("@PhotoData", student.PhotoData);
            else
                cmd.Parameters.AddWithValue("@PhotoData", DBNull.Value);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ================= DELETE =================
        public void DeleteStudent(int studentId)
        {
            using SqlConnection conn = db.GetConnection();

            string query = "DELETE FROM Students WHERE StudentId = @StudentId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ================= ADD ATTACHMENT =================
        public void AddAttachment(StudentAttachment attachment)
        {
            using SqlConnection conn = db.GetConnection();

            string query = @"
            INSERT INTO StudentAttachments
            (
                StudentId,
                FileName,
                FileData,
                UploadDate
            )
            VALUES
            (
                @StudentId,
                @FileName,
                @FileData,
                @UploadDate
            )";

            using SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@StudentId", attachment.StudentId);
            cmd.Parameters.AddWithValue("@FileName", attachment.FileName);
            cmd.Parameters.AddWithValue("@FileData", attachment.FileData);
            cmd.Parameters.AddWithValue("@UploadDate", attachment.UploadDate);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ================= GET ATTACHMENTS =================
        public List<StudentAttachment> GetAttachments(int studentId)
        {
            var list = new List<StudentAttachment>();

            using SqlConnection conn = db.GetConnection();

            string query = @"
            SELECT 
                AttachmentId,
                StudentId,
                FileName,
                FileData,
                UploadDate
            FROM StudentAttachments
            WHERE StudentId = @StudentId
            ORDER BY UploadDate DESC";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new StudentAttachment
                {
                    AttachmentId = (int)reader["AttachmentId"],
                    StudentId = (int)reader["StudentId"],
                    FileName = reader["FileName"].ToString(),
                    FileData = (byte[])reader["FileData"],
                    UploadDate = (DateTime)reader["UploadDate"]
                });
            }

            return list;
        }

        // ================= GET ATTACHMENT COUNT =================
        public int GetAttachmentCount(int studentId)
        {
            using SqlConnection conn = db.GetConnection();

            string query = @"
            SELECT COUNT(*) 
            FROM StudentAttachments 
            WHERE StudentId = @StudentId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        // ================= DELETE ATTACHMENT =================
        public void DeleteAttachment(int attachmentId)
        {
            using SqlConnection conn = db.GetConnection();

            string query = @"
            DELETE FROM StudentAttachments 
            WHERE AttachmentId = @AttachmentId";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@AttachmentId", attachmentId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}