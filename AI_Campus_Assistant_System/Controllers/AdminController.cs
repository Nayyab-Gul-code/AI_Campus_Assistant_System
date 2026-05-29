using AI_Campus_Assistant.Models;
using AI_Campus_Assistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml;
namespace AI_Campus_Assistant.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly MongoDbService _db;
        private readonly GroqAiService _ai;

        public AdminController(MongoDbService db, GroqAiService ai)
        {
            _db = db;
            _ai = ai;
        }

        // ==========================================
        // 核心 DASHBOARD (Optimized with Parallel Execution)
        // ==========================================
        public async Task<IActionResult> Dashboard()
        {
            // Running queries in parallel to drastically improve loading performance
            var studentCountTask = _db.CountUsersByRoleAsync("student");
            var teacherCountTask = _db.CountUsersByRoleAsync("teacher");
            var coursesTask = _db.GetAllCoursesAsync();
            var deptsTask = _db.GetAllDepartmentsAsync();
            var todayChatsTask = _db.CountTodayChatQueriesAsync();
            var complaintsTask = _db.GetAllComplaintsAsync();
            var timetablesTask = _db.GetAllTimetablesAsync();
            var feesTask = _db.GetAllFeesAsync();
            var feeRequestsTask = _db.CountPendingFeeRequestsAsync();
            var usersTask = _db.GetAllUsersAsync();
            var notificationsTask = _db.GetAllNotificationsAsync();

            await Task.WhenAll(
                studentCountTask, teacherCountTask, coursesTask, deptsTask,
                todayChatsTask, complaintsTask, timetablesTask, feesTask,
                feeRequestsTask, usersTask, notificationsTask
            );

            // Mapping to ViewBags
            ViewBag.TotalStudents = studentCountTask.Result;
            ViewBag.TotalTeachers = teacherCountTask.Result;
            ViewBag.TotalCourses = coursesTask.Result.Count;
            ViewBag.TotalDepts = deptsTask.Result.Count;
            ViewBag.TodayChats = todayChatsTask.Result;
            ViewBag.PendingComplaints = complaintsTask.Result.Count(c => c.Status == "pending");
            ViewBag.TotalTimetables = timetablesTask.Result.Count;

            var fees = feesTask.Result;
            ViewBag.TotalFees = fees.Count;
            ViewBag.UnpaidFees = fees.Count(f => f.Status == "Unpaid");
            ViewBag.PendingVerification = fees.Count(f => f.Status == "Pending Verification");
            ViewBag.PendingFeeRequests = feeRequestsTask.Result;

            ViewBag.RecentUsers = usersTask.Result.Take(5).ToList();
            ViewBag.Notifications = notificationsTask.Result.Take(5).ToList();

            return View();
        }

        // ==========================================
        // USER MANAGEMENT
        // ==========================================
        public async Task<IActionResult> Users(string? role, string? search)
        {
            var users = await _db.GetAllUsersAsync();

            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Role == role).ToList();

            if (!string.IsNullOrEmpty(search))
                users = users.Where(u =>
                    u.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.Filter = role;
            ViewBag.Search = search;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            ViewBag.Departments = await _db.GetAllDepartmentsAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Password cannot be empty.";
                ViewBag.Departments = await _db.GetAllDepartmentsAsync();
                return View(user);
            }

            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.Email = user.Email.ToLower().Trim();
                user.IsActive = true;
                user.CreatedAt = DateTime.UtcNow;

                await _db.CreateUserAsync(user);
                TempData["Success"] = "User created successfully!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating user: {ex.Message}";
                ViewBag.Departments = await _db.GetAllDepartmentsAsync();
                return View(user);
            }
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

            user.Id = id;
            user.PasswordHash = existing.PasswordHash; // Keep existing security stamp
            user.Email = user.Email.ToLower().Trim();

            await _db.UpdateUserAsync(id, user);
            TempData["Success"] = "User profile updated!";
            return RedirectToAction("Users");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _db.DeleteUserAsync(id);
            TempData["Success"] = "User removed from system.";
            return RedirectToAction("Users");
        }

        // ==========================================
        // DEPARTMENTS
        // ==========================================
        public async Task<IActionResult> Departments() => View(await _db.GetAllDepartmentsAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(Department dept)
        {
            dept.CreatedAt = DateTime.UtcNow;
            dept.IsActive = true;
            await _db.CreateDepartmentAsync(dept);
            TempData["Success"] = "Department onboarding complete!";
            return RedirectToAction("Departments");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(Department dept)
        {
            await _db.UpdateDepartmentAsync(dept.Id!, dept);
            TempData["Success"] = "Department data modified.";
            return RedirectToAction("Departments");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(string id)
        {
            await _db.DeleteDepartmentAsync(id);
            TempData["Success"] = "Department purged.";
            return RedirectToAction("Departments");
        }

        // ==========================================
        // COURSES MANAGEMENT
        // ==========================================
        public async Task<IActionResult> Courses()
        {
            ViewBag.Courses = await _db.GetAllCoursesAsync();
            ViewBag.Departments = await _db.GetAllDepartmentsAsync();
            ViewBag.Teachers = await _db.GetUsersByRoleAsync("teacher");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            ViewBag.Departments = await _db.GetAllDepartmentsAsync();
            ViewBag.Teachers = await _db.GetUsersByRoleAsync("teacher");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            course.IsActive = true;
            await _db.CreateCourseAsync(course);
            TempData["Success"] = "Course curriculum published!";
            return RedirectToAction("Courses");
        }

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
        {
            course.Id = id;
            await _db.UpdateCourseAsync(id, course);
            TempData["Success"] = "Course dynamics updated!";
            return RedirectToAction("Courses");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(string id)
        {
            await _db.DeleteCourseAsync(id);
            TempData["Success"] = "Course deprecated.";
            return RedirectToAction("Courses");
        }

        // ==========================================
        // TIMETABLE SCHEDULER
        // ==========================================
        public async Task<IActionResult> Timetable(string? program, int? semNo)
        {
            var all = await _db.GetAllTimetablesAsync();
            if (!string.IsNullOrEmpty(program)) all = all.Where(t => t.Program == program).ToList();
            if (semNo.HasValue) all = all.Where(t => t.SemesterNo == semNo.Value).ToList();

            ViewBag.Courses = await _db.GetAllCoursesAsync();
            ViewBag.Teachers = await _db.GetUsersByRoleAsync("teacher");
            ViewBag.FilterProgram = program;
            ViewBag.FilterSem = semNo;
            ViewBag.Days = TimeSlots.ValidDays;
            return View(all);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTimetable(Timetable tt)
        {
            // Business Rule Validation
            if (tt.DayOfWeek == "Saturday" || tt.DayOfWeek == "Sunday")
            {
                TempData["Error"] = "Weekend schedules are locked. Saturday and Sunday are off-days.";
                return RedirectToAction("Timetable");
            }
            if (tt.DayOfWeek == "Friday" && TimeSpan.TryParse(tt.StartTime, out var start) && start >= TimeSpan.FromHours(13) && start < TimeSpan.FromHours(14))
            {
                TempData["Error"] = "Operation failed: Juma Prayer interval reservation (1PM–2PM).";
                return RedirectToAction("Timetable");
            }
            if (TimeSpan.TryParse(tt.StartTime, out var s) && (s < TimeSpan.FromHours(8) || s >= TimeSpan.FromHours(15)))
            {
                TempData["Error"] = "Scheduling Error: Operational hours bounds are 8AM–3PM.";
                return RedirectToAction("Timetable");
            }

            tt.TimeSlot = $"{tt.StartTime}-{tt.EndTime}";
            tt.CreatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(tt.CourseId))
            {
                var course = await _db.GetCourseByIdAsync(tt.CourseId);
                if (course != null)
                {
                    tt.CourseName = course.Title;
                    tt.CourseCode = course.Code;
                    tt.Program = course.Program;
                    tt.SemesterNo = course.SemesterNo;
                    tt.TeacherId = course.TeacherId;
                }
            }

            if (!string.IsNullOrEmpty(tt.TeacherId))
            {
                var teacher = await _db.GetUserByIdAsync(tt.TeacherId);
                if (teacher != null) tt.TeacherName = teacher.Name;
            }

            await _db.CreateTimetableAsync(tt);
            TempData["Success"] = "Timetable layout updated!";
            return RedirectToAction("Timetable");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTimetable(string id)
        {
            await _db.DeleteTimetableAsync(id);
            TempData["Success"] = "Schedule slot dynamic freed.";
            return RedirectToAction("Timetable");
        }

        // ==========================================
        // ACADEMIC LEDGER & FINANCES
        // ==========================================
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
            ViewBag.FeeRequests = await _db.GetPendingFeeRequestsAsync();
            return View(fees);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFeeFromRequest(string requestId, string studentId, double tuitionFee, double libraryFee, double transportFee, double hostelFee, bool hasTransport, bool hasHostel, DateTime dueDate)
        {
            var student = await _db.GetUserByIdAsync(studentId);
            if (student == null) { TempData["Error"] = "Student structural schema lookup failed."; return RedirectToAction("Fees"); }

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
            TempData["Success"] = $"Invoice tracking standard bound for {student.Name}! ID: {fee.FeeId}";
            return RedirectToAction("Fees");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFee(Fee fee)
        {
            var student = await _db.GetUserByIdAsync(fee.StudentId);
            if (student != null)
            {
                fee.StudentName = student.Name;
                fee.Program = student.Program ?? "";
                fee.SemesterNo = student.SemesterNo;
            }
            fee.CreatedAt = DateTime.UtcNow;
            if (fee.DueDate == default) fee.DueDate = DateTime.Today.AddMonths(1);

            await _db.CreateFeeAsync(fee);
            TempData["Success"] = $"Fee architecture transactional sequence bound! ID: {fee.FeeId}";
            return RedirectToAction("Fees");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFeePaid(string id)
        {
            await _db.UpdateFeeStatusAsync(id, "Paid");
            TempData["Success"] = "Transaction reconciled. State: Paid.";
            return RedirectToAction("Fees");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFeeUnpaid(string id)
        {
            await _db.UpdateFeeStatusAsync(id, "Unpaid");
            TempData["Success"] = "Reversal committed. State: Unpaid.";
            return RedirectToAction("Fees");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFee(string id)
        {
            await _db.DeleteFeeAsync(id);
            TempData["Success"] = "Financial transaction instance deleted.";
            return RedirectToAction("Fees");
        }

        // ==========================================
        // GRADES & EXAM PERFORMANCES
        // ==========================================
        public async Task<IActionResult> Grades(string? program, int? semNo)
        {
            var courses = await _db.GetAllCoursesAsync();
            if (!string.IsNullOrEmpty(program)) courses = courses.Where(c => c.Program == program).ToList();
            if (semNo.HasValue) courses = courses.Where(c => c.SemesterNo == semNo.Value).ToList();

            var allGrades = new List<Grade>();
            foreach (var c in courses)
                allGrades.AddRange(await _db.GetGradesByCourseAsync(c.Id!));

            ViewBag.FilterProgram = program;
            ViewBag.FilterSem = semNo;
            return View(allGrades);
        }

        // ==========================================
        // PERFORMANCE CARD ENGINE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> StudentResultCard(string? studentId, int? semesterNo)
        {
            var students = await _db.GetUsersByRoleAsync("student");
            ViewBag.Students = students;
            ViewBag.SelectedStudentId = studentId;
            ViewBag.SelectedSemester = semesterNo;

            if (!string.IsNullOrEmpty(studentId))
            {
                var student = await _db.GetUserByIdAsync(studentId);
                if (student == null) { TempData["Error"] = "Student entity reference lost."; return View(); }

                var allGrades = await _db.GetGradesByStudentAsync(studentId);
                var targetSemester = semesterNo ?? student.SemesterNo;

                var semGrades = allGrades.Where(g => g.SemesterNo == targetSemester).ToList();
                var regCourses = await _db.GetCoursesByProgramSemesterAsync(student.Program ?? "", targetSemester);
                var incomplete = semGrades.Where(g => g.GradeLetter == "N/A" || g.TotalMarks == 0).ToList();

                ViewBag.Student = student;
                ViewBag.Grades = semGrades;
                ViewBag.IncompleteGrades = incomplete;
                ViewBag.IsComplete = semGrades.Any() && !incomplete.Any() && semGrades.Count >= regCourses.Count;
                ViewBag.TotalCourses = regCourses.Count;
                ViewBag.GradedCourses = semGrades.Count(g => g.GradeLetter != "N/A" && g.TotalMarks > 0);
                ViewBag.AllSemesters = allGrades.Select(g => g.SemesterNo).Distinct().OrderBy(x => x).ToList();

                if (semGrades.Any())
                {
                    var valid = semGrades.Where(g => g.GradeLetter != "N/A").ToList();
                    ViewBag.SGPA = valid.Any() ? Math.Round(valid.Average(g => g.GPA), 2) : 0.0;
                    ViewBag.ObtainedMarks = semGrades.Sum(g => g.TotalMarks);
                    ViewBag.TotalMaxMarks = semGrades.Sum(g => g.MaxTotal > 0 ? g.MaxTotal : 100);
                }
            }
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateResultCard(string studentId, int semesterNo)
        {
            var student = await _db.GetUserByIdAsync(studentId);
            if (student == null)
            {
                TempData["Error"] = "Critical: Context missing. Student profile context dropped.";
                return RedirectToAction("StudentResultCard");
            }

            var allGrades = await _db.GetGradesByStudentAsync(studentId);
            var semGrades = allGrades.Where(g => g.SemesterNo == semesterNo).ToList();

            if (!semGrades.Any())
            {
                TempData["Error"] = $"No active records captured for Semester {semesterNo}.";
                return RedirectToAction("StudentResultCard", new { studentId, semesterNo });
            }

            var incomplete = semGrades.Where(g => g.GradeLetter == "N/A" || g.TotalMarks == 0).ToList();
            if (incomplete.Any())
            {
                TempData["Error"] = $"Integrity Failure: Compilation missing dependencies ({incomplete.Count} fields unresolved). Check: " +
                    string.Join(", ", incomplete.Select(g => g.CourseName));
                return RedirectToAction("StudentResultCard", new { studentId, semesterNo });
            }

            try
            {
                foreach (var grade in semGrades)
                {
                    grade.IsReleased = true;
                    await _db.UpdateGradeAsync(grade.Id!, grade);
                }
                TempData["Success"] = $"Semester {semesterNo} matrix published successfully for user: {student.Name}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Critical Gateway Interface Exception: " + ex.Message;
            }

            return RedirectToAction("StudentResultCard", new { studentId, semesterNo });
        }

        // ==========================================
        // NOTIFICATIONS HUB
        // ==========================================
        public async Task<IActionResult> Notifications() => View(await _db.GetAllNotificationsAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNotification(Notification n)
        {
            n.CreatedAt = DateTime.UtcNow;
            await _db.CreateNotificationAsync(n);
            TempData["Success"] = "Global matrix broadcast dispatched!";
            return RedirectToAction("Notifications");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            await _db.DeleteNotificationAsync(id);
            TempData["Success"] = "Broadcast session terminated.";
            return RedirectToAction("Notifications");
        }

        // ==========================================
        // AI MONITORING ENGINE & KNOWLEDGE BASES
        // ==========================================
        public async Task<IActionResult> Complaints() => View(await _db.GetAllComplaintsAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveComplaint(string id, string adminReply)
        {
            await _db.ResolveComplaintAsync(id, adminReply);
            TempData["Success"] = "Ticket status evaluated: Resolved.";
            return RedirectToAction("Complaints");
        }

        public async Task<IActionResult> AiMonitor()
        {
            ViewBag.Queries = await _db.GetAllChatQueriesAsync();
            ViewBag.TodayCount = await _db.CountTodayChatQueriesAsync();
            return View();
        }

        public async Task<IActionResult> FAQs() => View(await _db.GetAllFaqsAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFAQ(FAQ faq)
        {
            faq.CreatedAt = DateTime.UtcNow;
            faq.IsActive = true;
            await _db.CreateFaqAsync(faq);
            TempData["Success"] = "Context model injected into standard dataset!";
            return RedirectToAction("FAQs");
        }

        [HttpGet]
        public async Task<IActionResult> EditFAQ(string id)
        {
            var faq = await _db.GetFaqByIdAsync(id);
            if (faq == null) return NotFound();
            return View(faq);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFAQ(string id, FAQ faq)
        {
            var existing = await _db.GetFaqByIdAsync(id);
            if (existing == null) return NotFound();

            faq.Id = id;
            faq.CreatedAt = existing.CreatedAt;
            await _db.UpdateFaqAsync(id, faq);
            TempData["Success"] = "Dataset nodes adjusted.";
            return RedirectToAction("FAQs");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFAQ(string id)
        {
            await _db.DeleteFaqAsync(id);
            TempData["Success"] = "FAQ item dropped from cache.";
            return RedirectToAction("FAQs");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFAQ(string id)
        {
            var faq = await _db.GetFaqByIdAsync(id);
            if (faq == null) return NotFound();

            faq.IsActive = !faq.IsActive;
            await _db.UpdateFaqAsync(id, faq);
            TempData["Success"] = faq.IsActive ? "FAQ online." : "FAQ pipeline hidden.";
            return RedirectToAction("FAQs");
        }
    }
}