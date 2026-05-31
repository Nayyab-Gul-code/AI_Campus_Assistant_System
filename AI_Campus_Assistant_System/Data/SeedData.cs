using AI_Campus_Assistant.Data;
using AI_Campus_Assistant.Models;
using MongoDB.Driver;

namespace AI_Campus_Assistant.Data
{
    public class SeedData
    {
        public static async Task SeedAsync(MongoDbContext db)
        {
            // ── 1. ADMIN USER ──────────────────────────────────────────
            var adminCount = await db.Users.CountDocumentsAsync(
                Builders<User>.Filter.Eq(u => u.Role, "admin"));

            if (adminCount == 0)
            {
                await db.Users.InsertOneAsync(new User
                {
                    Name        = "System Admin",
                    Email       = "admin@admin.com",
                    PasswordHash= BCrypt.Net.BCrypt.HashPassword("Admin1234"),
                    Role        = "admin",
                    Phone       = "+92-300-0000000",
                    IsActive    = true,
                    CreatedAt   = DateTime.UtcNow
                });
            }

            // ── 2. DEPARTMENTS ─────────────────────────────────────────
            var deptCount = await db.Departments.CountDocumentsAsync(
                Builders<Department>.Filter.Empty);

            if (deptCount == 0)
            {
                var departments = new List<Department>
                {
                    new Department { Name="BSCS",  Code="CS",  Description="Bachelor of Science in Computer Science", HeadName="Dr. Ahmed Ali",    TotalSemesters=8, IsActive=true },
                    new Department { Name="BSCE",  Code="CE",  Description="Bachelor of Science in Computer Engineering", HeadName="Dr. Sara Khan", TotalSemesters=8, IsActive=true },
                    new Department { Name="BSIT",  Code="IT",  Description="Bachelor of Science in Information Technology", HeadName="Dr. Usman Raza",TotalSemesters=8, IsActive=true },
                    new Department { Name="MS CS", Code="MSC", Description="Master of Science in Computer Science", HeadName="Prof. Ayesha Mir",   TotalSemesters=4, IsActive=true },
                    new Department { Name="MBA",   Code="MBA", Description="Master of Business Administration", HeadName="Dr. Bilal Sheikh",        TotalSemesters=4, IsActive=true },
                };
                await db.Departments.InsertManyAsync(departments);
            }

            // ── 3. SEMESTERS ───────────────────────────────────────────
            var semCount = await db.Semesters.CountDocumentsAsync(
                Builders<Semester>.Filter.Empty);

            if (semCount == 0)
            {
                await db.Semesters.InsertManyAsync(new List<Semester>
                {
                    new Semester { Name="Fall 2025",   StartDate=new DateTime(2025,9,1),  EndDate=new DateTime(2026,1,31), IsActive=false },
                    new Semester { Name="Spring 2026", StartDate=new DateTime(2026,2,1),  EndDate=new DateTime(2026,6,30), IsActive=true  },
                    new Semester { Name="Fall 2026",   StartDate=new DateTime(2026,9,1),  EndDate=new DateTime(2027,1,31), IsActive=false },
                });
            }

            // ── 4. DEMO TEACHER ────────────────────────────────────────
            var teacherExists = await db.Users.Find(
                Builders<User>.Filter.Eq(u => u.Email, "teacher@demo.com")).AnyAsync();

            if (!teacherExists)
            {
                await db.Users.InsertOneAsync(new User
                {
                    Name        = "Dr. Ahmed Hassan",
                    Email       = "teacher@demo.com",
                    PasswordHash= BCrypt.Net.BCrypt.HashPassword("Teacher1234"),
                    Role        = "teacher",
                    Phone       = "+92-301-1111111",
                    Designation = "Assistant Professor",
                    Program     = "BSCS",
                    Bio         = "PhD Computer Science, 10 years teaching experience.",
                    IsActive    = true,
                    CreatedAt   = DateTime.UtcNow
                });
            }

            // ── 5. DEMO STUDENT ────────────────────────────────────────
            var studentExists = await db.Users.Find(
                Builders<User>.Filter.Eq(u => u.Email, "student@demo.com")).AnyAsync();

            if (!studentExists)
            {
                await db.Users.InsertOneAsync(new User
                {
                    Name        = "Ali Raza",
                    Email       = "student@demo.com",
                    PasswordHash= BCrypt.Net.BCrypt.HashPassword("Student1234"),
                    Role        = "student",
                    Phone       = "+92-302-2222222",
                    Program     = "BSCS",
                    SemesterNo  = 3,
                    IsActive    = true,
                    CreatedAt   = DateTime.UtcNow
                });
            }

            // ── 6. COURSES ─────────────────────────────────────────────
            var courseCount = await db.Courses.CountDocumentsAsync(
                Builders<Course>.Filter.Empty);

            if (courseCount == 0)
            {
                var teacher = await db.Users.Find(
                    Builders<User>.Filter.Eq(u => u.Email, "teacher@demo.com")).FirstOrDefaultAsync();
                var teacherId   = teacher?.Id ?? "";
                var teacherName = teacher?.Name ?? "Dr. Ahmed Hassan";

                var courses = new List<Course>
                {
                    new Course { Title="Programming Fundamentals", Code="CS-101", DepartmentName="BSCS", Program="BSCS", SemesterNo=1, TeacherId=teacherId, TeacherName=teacherName, Credits=3 },
                    new Course { Title="Object Oriented Programming", Code="CS-201", DepartmentName="BSCS", Program="BSCS", SemesterNo=3, TeacherId=teacherId, TeacherName=teacherName, Credits=3 },
                    new Course { Title="Data Structures", Code="CS-202", DepartmentName="BSCS", Program="BSCS", SemesterNo=3, TeacherId=teacherId, TeacherName=teacherName, Credits=3 },
                    new Course { Title="Database Systems", Code="CS-301", DepartmentName="BSCS", Program="BSCS", SemesterNo=5, TeacherId=teacherId, TeacherName=teacherName, Credits=3 },
                    new Course { Title="Software Engineering", Code="CS-401", DepartmentName="BSCS", Program="BSCS", SemesterNo=7, TeacherId=teacherId, TeacherName=teacherName, Credits=3 },
                    new Course { Title="Advanced Algorithms", Code="MSC-501", DepartmentName="MS CS", Program="MS CS", SemesterNo=1, TeacherId=teacherId, TeacherName=teacherName, Credits=3 },
                };
                await db.Courses.InsertManyAsync(courses);
            }

            // ── 7. TIMETABLE ───────────────────────────────────────────
            var ttCount = await db.Timetables.CountDocumentsAsync(
                Builders<Timetable>.Filter.Empty);

            if (ttCount == 0)
            {
                var teacher = await db.Users.Find(
                    Builders<User>.Filter.Eq(u => u.Email, "teacher@demo.com")).FirstOrDefaultAsync();

                var timetables = new List<Timetable>
                {
                    new Timetable { CourseName="Object Oriented Programming", CourseCode="CS-201", TeacherName="Dr. Ahmed Hassan", TeacherId=teacher?.Id, DayOfWeek="Monday",    StartTime="08:00", EndTime="09:30", Room="CS-101", Program="BSCS", SemesterNo=3 },
                    new Timetable { CourseName="Data Structures",              CourseCode="CS-202", TeacherName="Dr. Ahmed Hassan", TeacherId=teacher?.Id, DayOfWeek="Monday",    StartTime="10:00", EndTime="11:30", Room="CS-102", Program="BSCS", SemesterNo=3 },
                    new Timetable { CourseName="Object Oriented Programming", CourseCode="CS-201", TeacherName="Dr. Ahmed Hassan", TeacherId=teacher?.Id, DayOfWeek="Wednesday",  StartTime="08:00", EndTime="09:30", Room="CS-101", Program="BSCS", SemesterNo=3 },
                    new Timetable { CourseName="Data Structures",              CourseCode="CS-202", TeacherName="Dr. Ahmed Hassan", TeacherId=teacher?.Id, DayOfWeek="Wednesday",  StartTime="10:00", EndTime="11:30", Room="CS-102", Program="BSCS", SemesterNo=3 },
                    new Timetable { CourseName="Object Oriented Programming", CourseCode="CS-201", TeacherName="Dr. Ahmed Hassan", TeacherId=teacher?.Id, DayOfWeek="Friday",     StartTime="08:00", EndTime="09:30", Room="CS-101", Program="BSCS", SemesterNo=3 },
                };
                await db.Timetables.InsertManyAsync(timetables);
            }

            // ── 8. NOTIFICATIONS ───────────────────────────────────────
            var notifCount = await db.Notifications.CountDocumentsAsync(
                Builders<Notification>.Filter.Empty);

            if (notifCount == 0)
            {
                await db.Notifications.InsertManyAsync(new List<Notification>
                {
                    new Notification { Title="Welcome to AI Campus!", Message="Your smart campus platform is now live. Explore AI Chat, Grades, Timetable and more.", TargetRole="all", Type="success" },
                    new Notification { Title="Mid-Term Exams Schedule", Message="Mid-term exams will begin from March 15, 2026. Check your timetable for details.", TargetRole="student", Type="warning" },
                    new Notification { Title="Assignment Submission Reminder", Message="Multiple assignments are due this week. Please submit before the deadline.", TargetRole="student", Type="info" },
                    new Notification { Title="Attendance Report Due", Message="Please submit monthly attendance reports by end of this week.", TargetRole="teacher", Type="warning" },
                });
            }

            // ── 9. FAQs ────────────────────────────────────────────────
            var faqCount = await db.Faqs.CountDocumentsAsync(
                Builders<FAQ>.Filter.Empty);

            if (faqCount == 0)
            {
                await db.Faqs.InsertManyAsync(new List<FAQ>
                {
                    new FAQ { Question="How do I check my grades?",          Answer="Go to Student Dashboard → Grades section to view all your grades.", Category="academics" },
                    new FAQ { Question="How to submit an assignment?",        Answer="Navigate to Assignments page, click on the assignment and submit your work.", Category="academics" },
                    new FAQ { Question="How do I check my attendance?",       Answer="Go to Attendance section from your dashboard to see your attendance record.", Category="attendance" },
                    new FAQ { Question="How to file a complaint?",            Answer="Go to Complaints section and click 'New Complaint' to submit your issue.", Category="support" },
                    new FAQ { Question="What is the minimum attendance required?", Answer="Minimum 75% attendance is required to appear in final exams.", Category="attendance" },
                    new FAQ { Question="How to contact my teacher?",          Answer="Go to your course page to find teacher contact information.", Category="support" },
                });
            }

            // ── 10. SAMPLE GRADES ──────────────────────────────────────
            var gradeCount = await db.Grades.CountDocumentsAsync(
                Builders<Grade>.Filter.Empty);

            if (gradeCount == 0)
            {
                var student = await db.Users.Find(
                    Builders<User>.Filter.Eq(u => u.Email, "student@demo.com")).FirstOrDefaultAsync();
                var teacher = await db.Users.Find(
                    Builders<User>.Filter.Eq(u => u.Email, "teacher@demo.com")).FirstOrDefaultAsync();

                if (student != null)
                {
                    await db.Grades.InsertManyAsync(new List<Grade>
                    {
                        new Grade { StudentId=student.Id!, StudentName=student.Name, CourseId="", CourseName="Object Oriented Programming", TeacherId=teacher?.Id ?? "", MidMarks=38, FinalMarks=72, AssignmentMarks=18, TotalMarks=88, GradeLetter="A", SemesterNo=3 },
                        new Grade { StudentId=student.Id!, StudentName=student.Name, CourseId="", CourseName="Data Structures",              TeacherId=teacher?.Id ?? "", MidMarks=35, FinalMarks=65, AssignmentMarks=16, TotalMarks=81, GradeLetter="B+", SemesterNo=3 },
                        new Grade { StudentId=student.Id!, StudentName=student.Name, CourseId="", CourseName="Calculus",                     TeacherId=teacher?.Id ?? "", MidMarks=30, FinalMarks=58, AssignmentMarks=14, TotalMarks=72, GradeLetter="B", SemesterNo=3 },
                    });
                }
            }

            // ── 11. SAMPLE ATTENDANCE ──────────────────────────────────
            var attCount = await db.Attendances.CountDocumentsAsync(
                Builders<Attendance>.Filter.Empty);

            if (attCount == 0)
            {
                var student = await db.Users.Find(
                    Builders<User>.Filter.Eq(u => u.Email, "student@demo.com")).FirstOrDefaultAsync();
                var teacher = await db.Users.Find(
                    Builders<User>.Filter.Eq(u => u.Email, "teacher@demo.com")).FirstOrDefaultAsync();

                if (student != null)
                {
                    var attendances = new List<Attendance>();
                    var statuses = new[] { "present","present","present","present","absent","present","present","late","present","present" };
                    for (int i = 0; i < 10; i++)
                    {
                        attendances.Add(new Attendance
                        {
                            StudentId   = student.Id!,
                            StudentName = student.Name,
                            CourseId    = "",
                            CourseName  = "Object Oriented Programming",
                            TeacherId   = teacher?.Id ?? "",
                            Date        = DateTime.UtcNow.AddDays(-i * 2),
                            Status      = statuses[i]
                        });
                    }
                    await db.Attendances.InsertManyAsync(attendances);
                }
            }
        }
    }
}
