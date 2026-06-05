using AI_Campus_Assistant.Models;
using AI_Campus_Assistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI_Campus_Assistant.Controllers
{
    [Authorize(Roles = "student")]
    public class StudentController : Controller
    {
        private readonly MongoDbService _db;
        private readonly AuthService _auth;
        private readonly GroqAiService _ai;
        private readonly IWebHostEnvironment _env;

        public StudentController(MongoDbService db, AuthService auth, GroqAiService ai, IWebHostEnvironment env)
        { _db = db; _auth = auth; _ai = ai; _env = env; }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        private string UserName => User.FindFirstValue(ClaimTypes.Name)!;

        // DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            var user = await _auth.GetUserByIdAsync(UserId);
            var grades = await _db.GetGradesByStudentAsync(UserId);
            var att = await _db.GetAttendanceByStudentAsync(UserId);
            var fees = await _db.GetFeesByStudentAsync(UserId);
            ViewBag.User = user;
            ViewBag.Grades = grades;
            ViewBag.Attendance = att;
            ViewBag.AttPct = att.Count > 0 ? att.Count(a => a.Status == "present") * 100.0 / att.Count : 0;
            ViewBag.Assignments = user != null
                ? await _db.GetAssignmentsByProgramSemesterAsync(user.Program ?? "", user.SemesterNo)
                : new List<Assignment>();
            ViewBag.Lectures = user != null
                ? (await _db.GetLecturesByProgramSemesterAsync(user.Program ?? "", user.SemesterNo)).Take(5).ToList()
                : new List<Lecture>();
            ViewBag.Fees = fees;
            ViewBag.UnpaidFees = fees.Count(f => f.Status == "Unpaid");
            ViewBag.Notifications = await _db.GetNotificationsForRoleAsync("student");
            return View();
        }

        // LECTURES
        public async Task<IActionResult> Lectures()
        {
            var user = await _auth.GetUserByIdAsync(UserId);
            ViewBag.User = user;
            var lectures = user != null
                ? await _db.GetLecturesByProgramSemesterAsync(user.Program ?? "", user.SemesterNo)
                : new List<Lecture>();
            return View(lectures);
        }

        public async Task<IActionResult> DownloadLecture(string id)
        {
            var lec = await _db.GetLectureByIdAsync(id);
            if (lec == null) return NotFound();
            var fp = Path.Combine(_env.WebRootPath, lec.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fp)) return NotFound("File not found.");
            return PhysicalFile(fp, GetContentType(lec.FileType), lec.FileName);
        }

        // ASSIGNMENTS
        public async Task<IActionResult> Assignments()
        {
            var user = await _auth.GetUserByIdAsync(UserId);
            var assignments = user != null
                ? await _db.GetAssignmentsByProgramSemesterAsync(user.Program ?? "", user.SemesterNo)
                : new List<Assignment>();
            var subs = await _db.GetSubmissionsByStudentAsync(UserId);
            ViewBag.SubmittedMap = subs.ToDictionary(s => s.AssignmentId, s => s);
            return View(assignments);
        }

        public async Task<IActionResult> DownloadAssignment(string id)
        {
            var asn = await _db.GetAssignmentByIdAsync(id);
            if (asn == null || string.IsNullOrEmpty(asn.FilePath)) return NotFound();
            var fp = Path.Combine(_env.WebRootPath, asn.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fp)) return NotFound("File not found.");
            return PhysicalFile(fp, GetContentType(asn.FileType), asn.FileName ?? "assignment");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignment(string assignmentId, string? content, IFormFile? submissionFile)
        {
            var existing = await _db.GetSubmissionByStudentAsync(assignmentId, UserId);
            if (existing != null)
            { TempData["Error"] = "You already submitted this assignment."; return RedirectToAction("Assignments"); }

            var asn = await _db.GetAssignmentByIdAsync(assignmentId);
            var sub = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                AssignmentTitle = asn?.Title ?? "",
                CourseId = asn?.CourseId ?? "",
                StudentId = UserId,
                StudentName = UserName,
                Content = content,
                TotalMarks = asn?.TotalMarks ?? 20,
                SubmittedAt = DateTime.UtcNow
            };

            if (submissionFile != null && submissionFile.Length > 0)
            {
                var allowed = new[] { ".pdf", ".docx", ".doc", ".pptx", ".ppt", ".xlsx", ".xls" };
                var ext = Path.GetExtension(submissionFile.FileName).ToLower();
                if (allowed.Contains(ext))
                {
                    var dir = Path.Combine(_env.WebRootPath, "uploads", "submissions");
                    Directory.CreateDirectory(dir);
                    var uname = $"{Guid.NewGuid()}{ext}";
                    using (var fs = new FileStream(Path.Combine(dir, uname), FileMode.Create))
                        await submissionFile.CopyToAsync(fs);
                    sub.FileName = submissionFile.FileName;
                    sub.FilePath = $"/uploads/submissions/{uname}";
                    sub.FileType = ext.TrimStart('.');
                    sub.FileSize = submissionFile.Length;
                }
            }
            await _db.SubmitAssignmentAsync(sub);
            TempData["Success"] = "Assignment submitted successfully!";
            return RedirectToAction("Assignments");
        }

        public async Task<IActionResult> DownloadMySubmission(string id)
        {
            var sub = await _db.GetSubmissionByIdAsync(id);
            if (sub == null || sub.StudentId != UserId || string.IsNullOrEmpty(sub.FilePath)) return NotFound();
            var fp = Path.Combine(_env.WebRootPath, sub.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fp)) return NotFound("File not found.");
            return PhysicalFile(fp, GetContentType(sub.FileType), sub.FileName ?? "submission");
        }

        // GRADES
        public async Task<IActionResult> Grades() => View(await _db.GetGradesByStudentAsync(UserId));

        // ── RESULT CARD (UPDATED WITH FIXES) ───────────────────────────
        public async Task<IActionResult> ResultCard(int semesterNo = 0)
        {
            var student = await _auth.GetUserByIdAsync(UserId);
            var allGrades = await _db.GetGradesByStudentAsync(UserId);
            var allSems = allGrades.Select(g => g.SemesterNo).Distinct().OrderBy(x => x).ToList();

            if (semesterNo == 0)
                semesterNo = student?.SemesterNo ?? 1;

            var semGrades = await _db.GetGradesByStudentSemesterAsync(UserId, semesterNo);

            // Metrics calculation
            double totalObtained = semGrades.Sum(g => g.TotalMarks);
            double totalMax = semGrades.Sum(g => g.MaxTotal);
            double calculatedSgpa = semGrades.Any() ? semGrades.Average(g => g.GPA) : 0.0;

            ViewBag.Student = student;
            ViewBag.SelectedSemester = semesterNo;
            ViewBag.AllSemesters = allSems;

            // Result release logic check (baki pages ki tarah true/false handle karne ke liye)
            ViewBag.IsReleased = semGrades.Any();

            ViewBag.SGPA = calculatedSgpa;
            ViewBag.ObtainedMarks = totalObtained;
            ViewBag.TotalMaxMarks = totalMax;
            ViewBag.Grades = semGrades;

            return View(semGrades);
        }

        // ATTENDANCE
        public async Task<IActionResult> Attendance()
        {
            var records = await _db.GetAttendanceByStudentAsync(UserId);
            ViewBag.TotalClasses = records.Count;
            ViewBag.Present = records.Count(a => a.Status == "present");
            ViewBag.Absent = records.Count(a => a.Status == "absent");
            ViewBag.Late = records.Count(a => a.Status == "late");
            ViewBag.Percentage = records.Count > 0
                ? records.Count(a => a.Status == "present") * 100.0 / records.Count : 0;
            return View(records);
        }

        // TIMETABLE
        public async Task<IActionResult> Timetable()
        {
            var user = await _auth.GetUserByIdAsync(UserId);
            ViewBag.User = user;
            var tt = user != null
                ? await _db.GetTimetableByProgramSemesterAsync(user.Program ?? "BSCS", user.SemesterNo)
                : new List<Timetable>();
            return View(tt);
        }

        // FEES
        public async Task<IActionResult> Fees()
        {
            var user = await _auth.GetUserByIdAsync(UserId);
            var fees = await _db.GetFeesByStudentAsync(UserId);
            var existingRequest = await _db.GetFeeRequestByStudentAsync(UserId);
            ViewBag.User = user;
            ViewBag.TotalDue = fees.Where(f => f.Status != "Paid").Sum(f => f.TotalFee);
            ViewBag.TotalPaid = fees.Where(f => f.Status == "Paid").Sum(f => f.TotalFee);
            ViewBag.ExistingRequest = existingRequest;
            return View(fees);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeeRequest(bool wantsTransport, bool wantsHostel, string? note)
        {
            var user = await _auth.GetUserByIdAsync(UserId);
            var existing = await _db.GetFeeRequestByStudentAsync(UserId);
            if (existing != null)
            { TempData["Error"] = "You already have a pending fee request."; return RedirectToAction("Fees"); }

            await _db.CreateFeeRequestAsync(new FeeRequest
            {
                StudentId = UserId,
                StudentName = UserName,
                Program = user?.Program ?? "",
                SemesterNo = user?.SemesterNo ?? 1,
                WantsTransport = wantsTransport,
                WantsHostel = wantsHostel,
                StudentNote = note,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });
            TempData["Success"] = "Fee preferences submitted! Admin will generate your challan shortly.";
            return RedirectToAction("Fees");
        }

        public async Task<IActionResult> FeeChallan(string id)
        {
            var fee = await _db.GetFeeByIdAsync(id);
            if (fee == null || fee.StudentId != UserId) return NotFound();
            return View(fee);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPaymentProof(string feeId, IFormFile? proofFile)
        {
            if (proofFile == null || proofFile.Length == 0)
            { TempData["Error"] = "Please select a file."; return RedirectToAction("Fees"); }

            var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(proofFile.FileName).ToLower();
            if (!allowed.Contains(ext))
            { TempData["Error"] = "Only PDF, JPG, PNG allowed."; return RedirectToAction("Fees"); }

            if (proofFile.Length > 5 * 1024 * 1024)
            { TempData["Error"] = "File must be less than 5 MB."; return RedirectToAction("Fees"); }

            var fee = await _db.GetFeeByIdAsync(feeId);
            if (fee == null || fee.StudentId != UserId)
            { TempData["Error"] = "Invalid fee record."; return RedirectToAction("Fees"); }

            if (fee.Status == "Paid")
            { TempData["Error"] = "This fee is already paid."; return RedirectToAction("Fees"); }

            if (fee.Status == "Pending Verification")
            { TempData["Error"] = "Proof already submitted, awaiting admin review."; return RedirectToAction("Fees"); }

            var dir = Path.Combine(_env.WebRootPath, "uploads", "payment-proofs");
            Directory.CreateDirectory(dir);
            var uname = $"{Guid.NewGuid()}{ext}";
            using (var fs = new FileStream(Path.Combine(dir, uname), FileMode.Create))
                await proofFile.CopyToAsync(fs);

            await _db.SubmitPaymentProofAsync(feeId, $"/uploads/payment-proofs/{uname}", proofFile.FileName);
            TempData["Success"] = "Payment proof submitted! Admin will verify shortly.";
            return RedirectToAction("Fees");
        }

        // AI CHAT
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
                    answer = $"📋 **{faqMatch.Question}**\n\n{faqMatch.Answer}";
                else
                    answer = await _ai.AskAsync(question);

                await _db.SaveChatQueryAsync(new ChatQuery
                { UserId = UserId, UserName = UserName, Question = question, Answer = answer, CreatedAt = DateTime.UtcNow });
            }
            return RedirectToAction("Chat");
        }

        // NOTIFICATIONS
        public async Task<IActionResult> Notifications() =>
            View(await _db.GetNotificationsForRoleAsync("student"));

        // COMPLAINTS
        public async Task<IActionResult> Complaints() => View(await _db.GetComplaintsByUserAsync(UserId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComplaint(string subject, string description)
        {
            await _db.CreateComplaintAsync(new Complaint
            {
                UserId = UserId,
                UserName = UserName,
                UserRole = "student",
                Subject = subject,
                Description = description,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            });
            TempData["Success"] = "Complaint submitted!";
            return RedirectToAction("Complaints");
        }

        // PROFILE
        public async Task<IActionResult> Profile() => View(await _auth.GetUserByIdAsync(UserId));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string name, string? phone, string? bio)
        {
            await _auth.UpdateProfileAsync(UserId, name, phone, bio, null);
            TempData["Success"] = "Profile updated!";
            return RedirectToAction("Profile");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            { TempData["Error"] = "Passwords do not match."; return RedirectToAction("Profile"); }
            var ok = await _auth.ChangePasswordAsync(UserId, currentPassword, newPassword);
            TempData[ok ? "Success" : "Error"] = ok ? "Password changed!" : "Wrong current password.";
            return RedirectToAction("Profile");
        }

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