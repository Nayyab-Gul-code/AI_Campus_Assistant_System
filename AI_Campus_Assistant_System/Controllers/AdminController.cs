using AI_Campus_Assistant.Models;
using AI_Campus_Assistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI_Campus_Assistant.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly MongoDbService _db;
        private readonly GeminiAiService _ai;
        public AdminController(MongoDbService db, GeminiAiService ai) { _db = db; _ai = ai; }

        // DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalStudents = await _db.CountUsersByRoleAsync("student");
            ViewBag.TotalTeachers = await _db.CountUsersByRoleAsync("teacher");
            ViewBag.TotalCourses = (await _db.GetAllCoursesAsync()).Count;
            ViewBag.TotalDepts = (await _db.GetAllDepartmentsAsync()).Count;
            ViewBag.TodayChats = await _db.CountTodayChatQueriesAsync();
            ViewBag.PendingComplaints = (await _db.GetAllComplaintsAsync()).Count(c => c.Status == "pending");
            ViewBag.TotalTimetables = (await _db.GetAllTimetablesAsync()).Count;
            var fees = await _db.GetAllFeesAsync();
            ViewBag.TotalFees = fees.Count;
            ViewBag.UnpaidFees = fees.Count(f => f.Status == "Unpaid");
            ViewBag.PendingVerification = fees.Count(f => f.Status == "Pending Verification");
            ViewBag.PendingFeeRequests = await _db.CountPendingFeeRequestsAsync();
            ViewBag.RecentUsers = (await _db.GetAllUsersAsync()).Take(5).ToList();
            ViewBag.Notifications = (await _db.GetAllNotificationsAsync()).Take(5).ToList();
            return View();
        }

        // USERS
        public async Task<IActionResult> Users(string? role, string? search)
        {
            var users = await _db.GetAllUsersAsync();
            if (!string.IsNullOrEmpty(role)) users = users.Where(u => u.Role == role).ToList();
            if (!string.IsNullOrEmpty(search)) users = users.Where(u =>
                u.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Filter = role; ViewBag.Search = search;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        { ViewBag.Departments = await _db.GetAllDepartmentsAsync(); return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user, string password)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.Email = user.Email.ToLower().Trim();
            user.IsActive = true; user.CreatedAt = DateTime.UtcNow;
            await _db.CreateUserAsync(user);
            TempData["Success"] = "User created!";
            return RedirectToAction("Users");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _db.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.Departments = await _db.GetAllDepartmentsAsync();
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, User user)
        {
            var existing = await _db.GetUserByIdAsync(id);
            if (existing == null) return NotFound();
            user.Id = id; user.PasswordHash = existing.PasswordHash;
            user.Email = user.Email.ToLower().Trim();
            await _db.UpdateUserAsync(id, user);
            TempData["Success"] = "User updated!";
            return RedirectToAction("Users");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        { await _db.DeleteUserAsync(id); TempData["Success"] = "User deleted."; return RedirectToAction("Users"); }

        // DEPARTMENTS
        public async Task<IActionResult> Departments() => View(await _db.GetAllDepartmentsAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(Department dept)
        { dept.CreatedAt = DateTime.UtcNow; dept.IsActive = true; await _db.CreateDepartmentAsync(dept); TempData["Success"] = "Department created!"; return RedirectToAction("Departments"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(Department dept)
        { await _db.UpdateDepartmentAsync(dept.Id!, dept); TempData["Success"] = "Department updated!"; return RedirectToAction("Departments"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(string id)
        { await _db.DeleteDepartmentAsync(id); TempData["Success"] = "Department deleted."; return RedirectToAction("Departments"); }

        // COURSES
        public async Task<IActionResult> Courses()
        {
            ViewBag.Courses = await _db.GetAllCoursesAsync();
            ViewBag.Departments = await _db.GetAllDepartmentsAsync();
            ViewBag.Teachers = await _db.GetUsersByRoleAsync("teacher");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        { ViewBag.Departments = await _db.GetAllDepartmentsAsync(); ViewBag.Teachers = await _db.GetUsersByRoleAsync("teacher"); return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        { course.CreatedAt = DateTime.UtcNow; course.IsActive = true; await _db.CreateCourseAsync(course); TempData["Success"] = "Course created!"; return RedirectToAction("Courses"); }

        [HttpGet]
        public async Task<IActionResult> EditCourse(string id)
        {
            var course = await _db.GetCourseByIdAsync(id);
            if (course == null) return NotFound();
            ViewBag.Departments = await _db.GetAllDepartmentsAsync();
            ViewBag.Teachers = await _db.GetUsersByRoleAsync("teacher");
            return View(course);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(string id, Course course)
        { course.Id = id; await _db.UpdateCourseAsync(id, course); TempData["Success"] = "Course updated!"; return RedirectToAction("Courses"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(string id)
        { await _db.DeleteCourseAsync(id); TempData["Success"] = "Course deleted."; return RedirectToAction("Courses"); }

        // TIMETABLE
        public async Task<IActionResult> Timetable(string? program, int? semNo)
        {
            var all = await _db.GetAllTimetablesAsync();
            if (!string.IsNullOrEmpty(program)) all = all.Where(t => t.Program == program).ToList();
            if (semNo.HasValue) all = all.Where(t => t.SemesterNo == semNo.Value).ToList();
            ViewBag.Courses = await _db.GetAllCoursesAsync();
            ViewBag.Teachers = await _db.GetUsersByRoleAsync("teacher");
            ViewBag.FilterProgram = program; ViewBag.FilterSem = semNo;
            ViewBag.Days = TimeSlots.ValidDays;
            return View(all);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTimetable(Timetable tt)
        {
            if (tt.DayOfWeek == "Saturday" || tt.DayOfWeek == "Sunday")
            { TempData["Error"] = "Saturday and Sunday are OFF days."; return RedirectToAction("Timetable"); }
            if (tt.DayOfWeek == "Friday" && TimeSpan.TryParse(tt.StartTime, out var start)
                && start >= TimeSpan.FromHours(13) && start < TimeSpan.FromHours(14))
            { TempData["Error"] = "Friday 1PM–2PM is Juma Break."; return RedirectToAction("Timetable"); }
            if (TimeSpan.TryParse(tt.StartTime, out var s) && (s < TimeSpan.FromHours(8) || s >= TimeSpan.FromHours(15)))
            { TempData["Error"] = "Classes only 8AM–3PM."; return RedirectToAction("Timetable"); }
            tt.TimeSlot = $"{tt.StartTime}-{tt.EndTime}"; tt.CreatedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(tt.CourseId))
            {
                var course = await _db.GetCourseByIdAsync(tt.CourseId);
                if (course != null) { tt.CourseName = course.Title; tt.CourseCode = course.Code; tt.Program = course.Program; tt.SemesterNo = course.SemesterNo; tt.TeacherId = course.TeacherId; }
            }
            if (!string.IsNullOrEmpty(tt.TeacherId))
            { var teacher = await _db.GetUserByIdAsync(tt.TeacherId); if (teacher != null) tt.TeacherName = teacher.Name; }
            await _db.CreateTimetableAsync(tt);
            TempData["Success"] = "Timetable entry added!";
            return RedirectToAction("Timetable");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTimetable(string id)
        { await _db.DeleteTimetableAsync(id); TempData["Success"] = "Entry removed."; return RedirectToAction("Timetable"); }

        // ── FEES ──────────────────────────────────────────────────────
        public async Task<IActionResult> Fees(string? status, string? search)
        {
            var fees = await _db.GetAllFeesAsync();
            if (!string.IsNullOrEmpty(status)) fees = fees.Where(f => f.Status == status).ToList();
            if (!string.IsNullOrEmpty(search)) fees = fees.Where(f =>
                f.StudentName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                f.FeeId.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.Students = await _db.GetUsersByRoleAsync("student");
            ViewBag.FilterStatus = status;
            ViewBag.Search = search;
            ViewBag.TotalAmount = fees.Sum(f => f.TotalFee);
            ViewBag.PaidAmount = fees.Where(f => f.Status == "Paid").Sum(f => f.TotalFee);
            ViewBag.UnpaidAmount = fees.Where(f => f.Status != "Paid").Sum(f => f.TotalFee);
            ViewBag.PendingVerification = fees.Count(f => f.Status == "Pending Verification");
            // Fee requests from students
            ViewBag.FeeRequests = await _db.GetPendingFeeRequestsAsync();
            return View(fees);
        }

        // Admin creates fee from a student fee request
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFeeFromRequest(
            string requestId, string studentId,
            double tuitionFee, double libraryFee,
            double transportFee, double hostelFee,
            bool hasTransport, bool hasHostel,
            DateTime dueDate)
        {
            var student = await _db.GetUserByIdAsync(studentId);
            if (student == null) { TempData["Error"] = "Student not found."; return RedirectToAction("Fees"); }

            var fee = new Fee
            {
                StudentId = studentId,
                StudentName = student.Name,
                Program = student.Program ?? "",
                SemesterNo = student.SemesterNo,
                TuitionFee = tuitionFee,
                LibraryFee = libraryFee,
                TransportFee = transportFee,
                HostelFee = hostelFee,
                HasTransport = hasTransport,
                HasHostel = hasHostel,
                DueDate = dueDate == default ? DateTime.Today.AddMonths(1) : dueDate,
                Status = "Unpaid",
                CreatedAt = DateTime.UtcNow
            };
            await _db.CreateFeeAsync(fee);
            await _db.MarkFeeRequestDoneAsync(requestId);
            TempData["Success"] = $"Fee challan created for {student.Name}! ID: {fee.FeeId}";
            return RedirectToAction("Fees");
        }

        // Admin creates fee manually (without request)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFee(Fee fee)
        {
            var student = await _db.GetUserByIdAsync(fee.StudentId);
            if (student != null) { fee.StudentName = student.Name; fee.Program = student.Program ?? ""; fee.SemesterNo = student.SemesterNo; }
            fee.CreatedAt = DateTime.UtcNow;
            if (fee.DueDate == default) fee.DueDate = DateTime.Today.AddMonths(1);
            await _db.CreateFeeAsync(fee);
            TempData["Success"] = $"Fee record created! ID: {fee.FeeId}";
            return RedirectToAction("Fees");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFeePaid(string id)
        { await _db.UpdateFeeStatusAsync(id, "Paid"); TempData["Success"] = "Fee marked as Paid."; return RedirectToAction("Fees"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFeeUnpaid(string id)
        { await _db.UpdateFeeStatusAsync(id, "Unpaid"); TempData["Success"] = "Fee marked as Unpaid."; return RedirectToAction("Fees"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFee(string id)
        { await _db.DeleteFeeAsync(id); TempData["Success"] = "Fee record deleted."; return RedirectToAction("Fees"); }

        // GRADES
        public async Task<IActionResult> Grades(string? program, int? semNo)
        {
            var courses = await _db.GetAllCoursesAsync();
            if (!string.IsNullOrEmpty(program)) courses = courses.Where(c => c.Program == program).ToList();
            if (semNo.HasValue) courses = courses.Where(c => c.SemesterNo == semNo.Value).ToList();
            var allGrades = new List<Grade>();
            foreach (var c in courses) allGrades.AddRange(await _db.GetGradesByCourseAsync(c.Id!));
            ViewBag.FilterProgram = program; ViewBag.FilterSem = semNo;
            return View(allGrades);
        }

        // NOTIFICATIONS
        public async Task<IActionResult> Notifications() => View(await _db.GetAllNotificationsAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNotification(Notification n)
        { n.CreatedAt = DateTime.UtcNow; await _db.CreateNotificationAsync(n); TempData["Success"] = "Notification sent!"; return RedirectToAction("Notifications"); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(string id)
        { await _db.DeleteNotificationAsync(id); TempData["Success"] = "Deleted."; return RedirectToAction("Notifications"); }

        // COMPLAINTS
        public async Task<IActionResult> Complaints() => View(await _db.GetAllComplaintsAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveComplaint(string id, string adminReply)
        { await _db.ResolveComplaintAsync(id, adminReply); TempData["Success"] = "Complaint resolved!"; return RedirectToAction("Complaints"); }

        // AI MONITOR
        public async Task<IActionResult> AiMonitor()
        { ViewBag.Queries = await _db.GetAllChatQueriesAsync(); ViewBag.TodayCount = await _db.CountTodayChatQueriesAsync(); return View(); }
    }
}
