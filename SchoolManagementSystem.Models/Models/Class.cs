
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SchoolManagementSystem.Models
{
    // ============================================================
    // CLASS (الصف الدراسي)
    // ============================================================
    public class Class
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int GradeLevel { get; set; }
        public string Section { get; set; }
        public int StudentCount { get; set; }
        public string ClassTeacherName { get; set; }

        public override string ToString() => ClassName;
    }
}