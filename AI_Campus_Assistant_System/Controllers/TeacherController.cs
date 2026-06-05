using AI_Campus_Assistant.Models;
using AI_Campus_Assistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI_Campus_Assistant.Controllers
{
    [Authorize(Roles = "teacher")]
    public class TeacherController : Controller
    {
        private readonly MongoDbService _db;
        private readonly AuthService _auth;
        private readonly GroqAiService _ai;
        private readonly IWebHostEnvironment _env;

        public TeacherController(MongoDbService db, AuthService auth,
            GroqAiService ai, IWebHostEnvironment env)
        { _db = db; _auth = auth; _ai = ai; _env = env; }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        private string UserName => User.FindFirstValue(ClaimTypes.Name)!;

        // ── DASHBOARD ─────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var courses = await _db.GetCoursesByTeacherAsync(UserId);
            var assignments = await _db.GetAssignmentsByTeacherAsync(UserId);
            var lectures = await _db.GetLecturesByTeacherAsync(UserId);
            var tt = await _db.GetTimetableByTeacherAsync(UserId);

            ViewBag.Courses = courses;
            ViewBag.TotalCourses = courses.Count;
            ViewBag.TotalAssignments = assignments.Count;
            ViewBag.TotalLectures = lectures.Count;
            ViewBag.Timetable = tt;
            ViewBag.Notifications = await _db.GetNotificationsForRoleAsync("teacher");
            return View();
        }

        // ── NOTIFICATIONS ─────────────────────────────────────────────
        public async Task<IActionResult> Notifications()
        {
            var notifications = await _db.GetNotificationsForRoleAsync("teacher");
            return View(notifications);
        }

        // ── COURSES ──────────────────────────────────────────────────
        public async Task<IActionResult> Courses() =>
            View(await _db.GetCoursesByTeacherAsync(UserId));

        // ── TIMETABLE ─────────────────────────────────────────────────
        public async Task<IActionResult> Timetable()
        {
            var tt = await _db.GetTimetableByTeacherAsync(UserId);
            return View(tt);
        }

        // ── LECTURES ─────────────────────────────────────────────────
        public async Task<IActionResult> Lectures()
        {
            ViewBag.Courses = await _db.GetCoursesByTeacherAsync(UserId);
            return View(await _db.GetLecturesByTeacherAsync(UserId));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLecture(
            string title, string? description, string courseId, int weekNo, IFormFile lectureFile)
        {
            if (lectureFile == null || lectureFile.Length == 0)
            { TempData["Error"] = "Please select a file."; return RedirectToAction("Lectures"); }

            var allowed = new[] { ".pdf", ".docx", ".doc", ".pptx", ".ppt", ".xlsx", ".xls" };
            var ext = Path.GetExtension(lectureFile.FileName).ToLower();
            if (!allowed.Contains(ext))
            { TempData["Error"] = "Only PDF, DOCX, PPTX, XLSX allowed."; return RedirectToAction("Lectures"); }

            var dir = Path.Combine(_env.WebRootPath, "uploads", "lectures");
            Directory.CreateDirectory(dir);
            var uname = $"{Guid.NewGuid()}{ext}";
            using (var fs = new FileStream(Path.Combine(dir, uname), FileMode.Create))
                await lectureFile.CopyToAsync(fs);

            var course = await _db.GetCourseByIdAsync(courseId);
            await _db.CreateLectureAsync(new Lecture
            {
                Title = title,
                Description = description,
                CourseId = courseId,
                CourseName = course?.Title ?? "",
                TeacherId = UserId,
                TeacherName = UserName,
                FileName = lectureFile.FileName,
                FilePath = $"/uploads/lectures/{uname}",
                FileType = ext.TrimStart('.'),
                FileSize = lectureFile.Length,
                Program = course?.Program ?? "",
                SemesterNo = course?.SemesterNo ?? 1,
                WeekNo = weekNo,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            TempData["Success"] = "Lecture uploaded!";
            return RedirectToAction("Lectures");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLecture(string id)
        {
            var lec = await _db.GetLectureByIdAsync(id);
            if (lec != null)
            {
                var fp = Path.Combine(_env.WebRootPath, lec.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fp)) System.IO.File.Delete(fp);
                await _db.DeleteLectureAsync(id);
            }
            TempData["Success"] = "Lecture deleted.";
            return RedirectToAction("Lectures");
        }

        public async Task<IActionResult> DownloadLecture(string id)
        {
            var lec = await _db.GetLectureByIdAsync(id);
            if (lec == null) return NotFound();
            var fp = Path.Combine(_env.WebRootPath, lec.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fp)) return NotFound("File not found.");
            return PhysicalFile(fp, GetContentType(lec.FileType), lec.FileName);
        }

        // ── ATTENDANCE ────────────────────────────────────────────────
        public async Task<IActionResult> Attendance()
        {
            ViewBag.Courses = await _db.GetCoursesByTeacherAsync(UserId);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsByCourse(string courseId)
        {
            var course = await _db.GetCourseByIdAsync(courseId);
            if (course == null) return Json(new List<object>());
            var students = await _db.GetStudentsByProgramSemesterAsync(course.Program, course.SemesterNo);
            return Json(students.Select(s => new { id = s.Id, name = s.Name }));
        }

        // ── MARK ATTENDANCE (FIXED) ───────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(
            string courseId, string courseName, string date,
            List<string> studentIds, List<string> studentNames, List<string> statuses)
        {
            // NULL SAFETY
            studentIds ??= new List<string>();
            studentNames ??= new List<string>();
            statuses ??= new List<string>();

            var parsedDate = DateTime.Parse(date);
            await _db.DeleteAttendanceByCourseAndDateAsync(courseId, parsedDate);

            // Safe count — teen lists mein se sabse choti ki size
            int count = Math.Min(studentIds.Count,
                        Math.Min(studentNames.Count, statuses.Count));

            var list = new List<Attendance>();
            for (int i = 0; i < count; i++)
                list.Add(new Attendance
                {
                    StudentId = studentIds[i],
                    StudentName = studentNames[i],
                    CourseId = courseId,
                    CourseName = courseName,
                    TeacherId = UserId,
                    Date = parsedDate,
                    Status = statuses[i]
                });

            await _db.MarkBulkAttendanceAsync(list);
            TempData["Success"] = $"Attendance saved for {list.Count} students!";
            return RedirectToAction("Attendance");
        }

        // ── ATTENDANCE REPORT ─────────────────────────────────────────
        public async Task<IActionResult> AttendanceReport(string? courseId)
        {
            var courses = await _db.GetCoursesByTeacherAsync(UserId);
            ViewBag.Courses = courses;
            if (!string.IsNullOrEmpty(courseId))
            {
                ViewBag.Records = await _db.GetAttendanceByCourseAllDatesAsync(courseId);
                ViewBag.SelCourse = courses.FirstOrDefault(c => c.Id == courseId);
            }
            return View();
        }

        // ── STUDENTS ─────────────────────────────────────────────────
        public async Task<IActionResult> Students()
        {
            var courses = await _db.GetCoursesByTeacherAsync(UserId);
            ViewBag.Courses = courses;
            var all = new List<User>();
            foreach (var c in courses)
                all.AddRange(await _db.GetStudentsByProgramSemesterAsync(c.Program, c.SemesterNo));
            return View(all.DistinctBy(s => s.Id).ToList());
        }

        // ── ASSIGNMENTS ──────────────────────────────────────────────
        public async Task<IActionResult> Assignments()
        {
            ViewBag.Courses = await _db.GetCoursesByTeacherAsync(UserId);
            return View(await _db.GetAssignmentsByTeacherAsync(UserId));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(
            string Title, string? Description, string CourseId,
            DateTime DueDate, int TotalMarks, IFormFile? assignmentFile)
        {
            var course = await _db.GetCourseByIdAsync(CourseId);
            var asn = new Assignment
            {
                Title = Title,
                Description = Description,
                CourseId = CourseId,
                CourseName = course?.Title ?? "",
                TeacherId = UserId,
                TeacherName = UserName,
                DueDate = DueDate,
                TotalMarks = TotalMarks,
                Program = course?.Program ?? "",
                SemesterNo = course?.SemesterNo ?? 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            if (assignmentFile != null && assignmentFile.Length > 0)
            {
                var allowed = new[] { ".pdf", ".docx", ".doc", ".pptx", ".ppt", ".xlsx", ".xls" };
                var ext = Path.GetExtension(assignmentFile.FileName).ToLower();
                if (allowed.Contains(ext))
                {
                    var dir = Path.Combine(_env.WebRootPath, "uploads", "assignments");
                    Directory.CreateDirectory(dir);
                    var uname = $"{Guid.NewGuid()}{ext}";
                    using (var fs = new FileStream(Path.Combine(dir, uname), FileMode.Create))
                        await assignmentFile.CopyToAsync(fs);
                    asn.FileName = assignmentFile.FileName;
                    asn.FilePath = $"/uploads/assignments/{uname}";
                    asn.FileType = ext.TrimStart('.');
                }
            }

            await _db.CreateAssignmentAsync(asn);
            TempData["Success"] = "Assignment created!";
            return RedirectToAction("Assignments");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssignment(string id)
        {
            await _db.DeleteAssignmentAsync(id);
            TempData["Success"] = "Assignment deleted.";
            return RedirectToAction("Assignments");
        }

        public async Task<IActionResult> DownloadAssignment(string id)
        {
            var asn = await _db.GetAssignmentByIdAsync(id);
            if (asn == null || string.IsNullOrEmpty(asn.FilePath)) return NotFound();
            var fp = Path.Combine(_env.WebRootPath, asn.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fp)) return NotFound("File not found.");
            return PhysicalFile(fp, GetContentType(asn.FileType), asn.FileName ?? "assignment");
        }

        // ── GRADE SUBMISSIONS ─────────────────────────────────────────
        public async Task<IActionResult> GradeSubmissions(string? assignmentId)
        {
            ViewBag.Assignments = await _db.GetAssignmentsByTeacherAsync(UserId);
            if (!string.IsNullOrEmpty(assignmentId))
            {
                ViewBag.SelectedAssignment = await _db.GetAssignmentByIdAsync(assignmentId);
                ViewBag.Submissions = await _db.GetSubmissionsByAssignmentAsync(assignmentId);
            }
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(string submissionId, double marks, string? feedback, string? assignmentId)
        {
            await _db.GradeSubmissionAsync(submissionId, marks, feedback);
            TempData["Success"] = "Grade saved!";
            return RedirectToAction("GradeSubmissions", new { assignmentId });
        }

        public async Task<IActionResult> DownloadSubmission(string id)
        {
            var sub = await _db.GetSubmissionByIdAsync(id);
            if (sub == null || string.IsNullOrEmpty(sub.FilePath)) return NotFound();
            var fp = Path.Combine(_env.WebRootPath, sub.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fp)) return NotFound("File not found.");
            return PhysicalFile(fp, GetContentType(sub.FileType), sub.FileName ?? "submission");
        }

        // ── GRADE STUDENTS ────────────────────────────────────────────
        public async Task<IActionResult> GradeStudents(string? courseId)
        {
            var courses = await _db.GetCoursesByTeacherAsync(UserId);
            ViewBag.Courses = courses;
            if (!string.IsNullOrEmpty(courseId))
            {
                var course = await _db.GetCourseByIdAsync(courseId);
                ViewBag.SelCourse = course;
                ViewBag.Students = course != null
                    ? await _db.GetStudentsByProgramSemesterAsync(course.Program, course.SemesterNo)
                    : new List<User>();
                ViewBag.Grades = await _db.GetGradesByCourseAsync(courseId);
            }
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrade(
            string studentId, string studentName, string courseId, string courseName,
            double midMarks, int midTotal,
            double finalMarks, int finalTotal,
            double assignmentMarks, int assignmentTotal,
            int semesterNo, string? remarks)
        {
            var grade = new Grade
            {
                StudentId = studentId,
                StudentName = studentName,
                CourseId = courseId,
                CourseName = courseName,
                TeacherId = UserId,
                MidMarks = midMarks,
                MidTotal = midTotal,
                FinalMarks = finalMarks,
                FinalTotal = finalTotal,
                AssignmentMarks = assignmentMarks,
                AssignmentTotal = assignmentTotal,
                SemesterNo = semesterNo,
                Remarks = remarks,
                CreatedAt = DateTime.UtcNow
            };
            await _db.UpsertGradeAsync(grade);
            TempData["Success"] = $"Grade saved for {studentName}!";
            return RedirectToAction("GradeStudents", new { courseId });
        }

        // ── PROFILE ──────────────────────────────────────────────────
        public async Task<IActionResult> Profile() =>
            View(await _auth.GetUserByIdAsync(UserId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string name, string? phone, string? bio, string? designation)
        {
            await _auth.UpdateProfileAsync(UserId, name, phone, bio, designation);
            TempData["Success"] = "Profile updated!";
            return RedirectToAction("Profile");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            { TempData["Error"] = "Passwords do not match."; return RedirectToAction("Profile"); }
            var ok = await _auth.ChangePasswordAsync(UserId, currentPassword, newPassword);
            TempData[ok ? "Success" : "Error"] = ok ? "Password changed!" : "Wrong current password.";
            return RedirectToAction("Profile");
        }

        // ── COMPLAINTS ───────────────────────────────────────────────
        public async Task<IActionResult> Complaints() =>
            View(await _db.GetComplaintsByUserAsync(UserId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComplaint(string subject, string description)
        {
            await _db.CreateComplaintAsync(new Complaint
            {
                UserId = UserId,
                UserName = UserName,
                UserRole = "teacher",
                Subject = subject,
                Description = description,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            });
            TempData["Success"] = "Complaint submitted!";
            return RedirectToAction("Complaints");
        }

        // ── CHAT ─────────────────────────────────────────────────────
        public async Task<IActionResult> Chat()
        {
            ViewBag.History = await _db.GetChatHistoryByUserAsync(UserId);
            ViewBag.Faqs = await _db.GetActiveFaqsAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat(string question)
        {
            if (!string.IsNullOrWhiteSpace(question))
            {
                var faqMatch = await _db.FindFaqByKeywordAsync(question);
                string answer;
                if (faqMatch != null)
                    answer = "\ud83d\udccb **" + faqMatch.Question + "**\n\n" + faqMatch.Answer;
                else
                    answer = await _ai.AskAsync(question);

                await _db.SaveChatQueryAsync(new ChatQuery
                { UserId = UserId, UserName = UserName, Question = question, Answer = answer, CreatedAt = DateTime.UtcNow });
            }
            return RedirectToAction("Chat");
        }

        // ── GENERATE QUIZ / ASSIGNMENT ────────────────────────────────
        public async Task<IActionResult> Generate()
        {
            ViewBag.Courses = await _db.GetCoursesByTeacherAsync(UserId);
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(string type, string topic, string courseId, int count, string difficulty, string quizFormat, string? downloadType)
        {
            ViewBag.Courses = await _db.GetCoursesByTeacherAsync(UserId);
            var course = !string.IsNullOrEmpty(courseId) ? await _db.GetCourseByIdAsync(courseId) : null;
            var courseName = course?.Title ?? "General";

            string prompt = (type == "quiz")
                ? (quizFormat == "mcq"
                    ? $"Generate {count} MCQs for {courseName}, Topic: {topic}. Provide 4 options (A,B,C,D) and Answer. \nFormat:\n---QUESTIONS---\nQ1. [Text]\nA) B) C) D)\n---SOLUTIONS---\nAnswer: [Letter]"
                    : $"Generate {count} subjective questions for {courseName}, Topic: {topic}. \nFormat:\n---QUESTIONS---\nQ1. [Question]\n---SOLUTIONS---\n[Detailed Answer]")
                : $"Create an assignment for {courseName}, Topic: {topic}. Format: ---QUESTIONS--- [Tasks] ---SOLUTIONS--- [Rubric]";

            try
            {
                var fullResult = await _ai.AskAsync(prompt);
                var parts = fullResult.Split("---SOLUTIONS---");
                var questions = parts[0].Replace("---QUESTIONS---", "").Trim();
                var solutions = parts.Length > 1 ? parts[1].Trim() : "No solutions provided.";

                if (downloadType == "questions")
                    return File(System.Text.Encoding.UTF8.GetBytes(questions), "text/plain", $"{type}_{topic}_Questions.txt");

                if (downloadType == "solutions")
                    return File(System.Text.Encoding.UTF8.GetBytes(solutions), "text/plain", $"{type}_{topic}_Solutions.txt");

                ViewBag.Generated = questions;
                ViewBag.Solutions = solutions;
                ViewBag.Type = type;
                ViewBag.Topic = topic;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "AI Service Error: " + ex.Message;
            }

            return View();
        }

        // ── HELPER ───────────────────────────────────────────────────
        private static string GetContentType(string? ext) => (ext ?? "").ToLower() switch
        {
            "pdf" => "application/pdf",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "doc" => "application/msword",
            "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "ppt" => "application/vnd.ms-powerpoint",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "xls" => "application/vnd.ms-excel",
            _ => "application/octet-stream"
        };
    }
}