using MongoDB.Driver;
using AI_Campus_Assistant.Data;
using AI_Campus_Assistant.Models;

namespace AI_Campus_Assistant.Services
{
    public class MongoDbService
    {
        private readonly MongoDbContext _db;
        public MongoDbService(MongoDbContext db) => _db = db;

        // ── USERS ─────────────────────────────────────────────────────
        public async Task<List<User>> GetAllUsersAsync() =>
            await _db.Users.Find(_ => true).SortByDescending(u => u.CreatedAt).ToListAsync();

        public async Task<List<User>> GetUsersByRoleAsync(string role) =>
            await _db.Users.Find(u => u.Role == role).SortBy(u => u.Name).ToListAsync();

        public async Task<User?> GetUserByIdAsync(string id) =>
            await _db.Users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task CreateUserAsync(User user) => await _db.Users.InsertOneAsync(user);

        public async Task UpdateUserAsync(string id, User user) =>
            await _db.Users.ReplaceOneAsync(u => u.Id == id, user);

        public async Task DeleteUserAsync(string id) =>
            await _db.Users.DeleteOneAsync(u => u.Id == id);

        public async Task<long> CountUsersByRoleAsync(string role) =>
            await _db.Users.CountDocumentsAsync(u => u.Role == role);

        public async Task<List<User>> GetStudentsByProgramSemesterAsync(string program, int semNo) =>
            await _db.Users.Find(u => u.Role == "student" && u.Program == program && u.SemesterNo == semNo)
                .SortBy(u => u.Name).ToListAsync();

        // ── DEPARTMENTS ───────────────────────────────────────────────
        public async Task<List<Department>> GetAllDepartmentsAsync() =>
            await _db.Departments.Find(_ => true).SortBy(d => d.Name).ToListAsync();

        public async Task<Department?> GetDepartmentByIdAsync(string id) =>
            await _db.Departments.Find(d => d.Id == id).FirstOrDefaultAsync();

        public async Task CreateDepartmentAsync(Department dept) => await _db.Departments.InsertOneAsync(dept);

        public async Task UpdateDepartmentAsync(string id, Department dept) =>
            await _db.Departments.ReplaceOneAsync(d => d.Id == id, dept);

        public async Task DeleteDepartmentAsync(string id) =>
            await _db.Departments.DeleteOneAsync(d => d.Id == id);

        // ── COURSES ──────────────────────────────────────────────────
        public async Task<List<Course>> GetAllCoursesAsync() =>
            await _db.Courses.Find(_ => true).SortBy(c => c.Title).ToListAsync();

        public async Task<List<Course>> GetCoursesByTeacherAsync(string teacherId) =>
            await _db.Courses.Find(c => c.TeacherId == teacherId).SortBy(c => c.Title).ToListAsync();

        public async Task<List<Course>> GetCoursesByProgramSemesterAsync(string program, int semNo) =>
            await _db.Courses.Find(c => c.Program == program && c.SemesterNo == semNo).ToListAsync();

        public async Task<Course?> GetCourseByIdAsync(string id) =>
            await _db.Courses.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task CreateCourseAsync(Course course) => await _db.Courses.InsertOneAsync(course);

        public async Task UpdateCourseAsync(string id, Course course) =>
            await _db.Courses.ReplaceOneAsync(c => c.Id == id, course);

        public async Task DeleteCourseAsync(string id) =>
            await _db.Courses.DeleteOneAsync(c => c.Id == id);

        // ── TIMETABLE ─────────────────────────────────────────────────
        public async Task<List<Timetable>> GetTimetableByProgramSemesterAsync(string program, int semNo) =>
            await _db.Timetables.Find(t => t.Program == program && t.SemesterNo == semNo)
                .SortBy(t => t.DayOfWeek).ThenBy(t => t.StartTime).ToListAsync();

        public async Task<List<Timetable>> GetTimetableByTeacherAsync(string teacherId) =>
            await _db.Timetables.Find(t => t.TeacherId == teacherId)
                .SortBy(t => t.DayOfWeek).ThenBy(t => t.StartTime).ToListAsync();

        public async Task<List<Timetable>> GetAllTimetablesAsync() =>
            await _db.Timetables.Find(_ => true)
                .SortBy(t => t.Program).ThenBy(t => t.SemesterNo)
                .ThenBy(t => t.DayOfWeek).ThenBy(t => t.StartTime).ToListAsync();

        public async Task CreateTimetableAsync(Timetable tt) => await _db.Timetables.InsertOneAsync(tt);

        public async Task UpdateTimetableAsync(string id, Timetable tt) =>
            await _db.Timetables.ReplaceOneAsync(t => t.Id == id, tt);

        public async Task DeleteTimetableAsync(string id) =>
            await _db.Timetables.DeleteOneAsync(t => t.Id == id);

        // ── ATTENDANCE ────────────────────────────────────────────────
        public async Task<List<Attendance>> GetAttendanceByStudentAsync(string studentId) =>
            await _db.Attendances.Find(a => a.StudentId == studentId)
                .SortByDescending(a => a.Date).ToListAsync();

        public async Task<List<Attendance>> GetAttendanceByCourseAllDatesAsync(string courseId) =>
            await _db.Attendances.Find(a => a.CourseId == courseId)
                .SortByDescending(a => a.Date).ToListAsync();

        public async Task<List<Attendance>> GetAttendanceByTeacherAsync(string teacherId) =>
            await _db.Attendances.Find(a => a.TeacherId == teacherId)
                .SortByDescending(a => a.Date).ToListAsync();

        public async Task DeleteAttendanceByCourseAndDateAsync(string courseId, DateTime date) =>
            await _db.Attendances.DeleteManyAsync(a => a.CourseId == courseId
                && a.Date >= date.Date && a.Date < date.Date.AddDays(1));

        public async Task MarkBulkAttendanceAsync(List<Attendance> list) =>
            await _db.Attendances.InsertManyAsync(list);

        // ── GRADES ────────────────────────────────────────────────────
        public async Task<List<Grade>> GetGradesByStudentAsync(string studentId) =>
            await _db.Grades.Find(g => g.StudentId == studentId)
                .SortByDescending(g => g.UpdatedAt).ToListAsync();

        public async Task<List<Grade>> GetGradesByCourseAsync(string courseId) =>
            await _db.Grades.Find(g => g.CourseId == courseId).ToListAsync();

        public async Task<List<Grade>> GetGradesByTeacherAsync(string teacherId) =>
            await _db.Grades.Find(g => g.TeacherId == teacherId).ToListAsync();

        public async Task<Grade?> GetGradeByStudentCourseAsync(string studentId, string courseId) =>
            await _db.Grades.Find(g => g.StudentId == studentId && g.CourseId == courseId)
                .FirstOrDefaultAsync();

        // ── NEW METHOD: semester wise grades ─────────────────────────
        public async Task<List<Grade>> GetGradesByStudentSemesterAsync(string studentId, int semNo) =>
            await _db.Grades.Find(g => g.StudentId == studentId && g.SemesterNo == semNo)
                .SortBy(g => g.CourseName).ToListAsync();

        // ── FIXED METHOD: Update single grade in MongoDB (Needed for Result Card Release) ──
        public async Task UpdateGradeAsync(string id, Grade grade) =>
            await _db.Grades.ReplaceOneAsync(g => g.Id == id, grade);

        public async Task UpsertGradeAsync(Grade grade)
        {
            grade.TotalMarks = grade.MidMarks + grade.FinalMarks + grade.AssignmentMarks;
            grade.MaxTotal = grade.MidTotal + grade.FinalTotal + grade.AssignmentTotal;
            grade.GradeLetter = Grade.CalculateGrade(grade.TotalMarks, grade.MaxTotal);
            grade.GPA = Grade.CalculateGPA(grade.GradeLetter);
            grade.UpdatedAt = DateTime.UtcNow;

            var existing = await _db.Grades.Find(g =>
                g.StudentId == grade.StudentId && g.CourseId == grade.CourseId).FirstOrDefaultAsync();

            if (existing != null)
            {
                grade.Id = existing.Id; grade.CreatedAt = existing.CreatedAt;
                await _db.Grades.ReplaceOneAsync(g => g.Id == existing.Id, grade);
            }
            else await _db.Grades.InsertOneAsync(grade);
        }

        // ── LECTURES ──────────────────────────────────────────────────
        public async Task<List<Lecture>> GetLecturesByTeacherAsync(string teacherId) =>
            await _db.Lectures.Find(l => l.TeacherId == teacherId)
                .SortByDescending(l => l.CreatedAt).ToListAsync();

        public async Task<List<Lecture>> GetLecturesByCourseAsync(string courseId) =>
            await _db.Lectures.Find(l => l.CourseId == courseId && l.IsActive)
                .SortBy(l => l.WeekNo).ToListAsync();

        public async Task<List<Lecture>> GetLecturesByProgramSemesterAsync(string program, int semNo) =>
            await _db.Lectures.Find(l => l.Program == program && l.SemesterNo == semNo && l.IsActive)
                .SortByDescending(l => l.CreatedAt).ToListAsync();

        public async Task<Lecture?> GetLectureByIdAsync(string id) =>
            await _db.Lectures.Find(l => l.Id == id).FirstOrDefaultAsync();

        public async Task CreateLectureAsync(Lecture lec) => await _db.Lectures.InsertOneAsync(lec);

        public async Task DeleteLectureAsync(string id) =>
            await _db.Lectures.DeleteOneAsync(l => l.Id == id);

        // ── ASSIGNMENTS ───────────────────────────────────────────────
        public async Task<List<Assignment>> GetAssignmentsByTeacherAsync(string teacherId) =>
            await _db.Assignments.Find(a => a.TeacherId == teacherId)
                .SortByDescending(a => a.CreatedAt).ToListAsync();

        public async Task<List<Assignment>> GetAssignmentsByProgramSemesterAsync(string program, int semNo) =>
            await _db.Assignments.Find(a => a.Program == program && a.SemesterNo == semNo && a.IsActive)
                .SortByDescending(a => a.DueDate).ToListAsync();

        public async Task<Assignment?> GetAssignmentByIdAsync(string id) =>
            await _db.Assignments.Find(a => a.Id == id).FirstOrDefaultAsync();

        public async Task CreateAssignmentAsync(Assignment a) => await _db.Assignments.InsertOneAsync(a);

        public async Task DeleteAssignmentAsync(string id) =>
            await _db.Assignments.DeleteOneAsync(a => a.Id == id);

        // ── SUBMISSIONS ───────────────────────────────────────────────
        public async Task<List<AssignmentSubmission>> GetSubmissionsByAssignmentAsync(string assignmentId) =>
            await _db.AssignmentSubmissions.Find(s => s.AssignmentId == assignmentId)
                .SortByDescending(s => s.SubmittedAt).ToListAsync();

        public async Task<AssignmentSubmission?> GetSubmissionByStudentAsync(string assignmentId, string studentId) =>
            await _db.AssignmentSubmissions.Find(s =>
                s.AssignmentId == assignmentId && s.StudentId == studentId).FirstOrDefaultAsync();

        public async Task<List<AssignmentSubmission>> GetSubmissionsByStudentAsync(string studentId) =>
            await _db.AssignmentSubmissions.Find(s => s.StudentId == studentId)
                .SortByDescending(s => s.SubmittedAt).ToListAsync();

        public async Task<AssignmentSubmission?> GetSubmissionByIdAsync(string id) =>
            await _db.AssignmentSubmissions.Find(s => s.Id == id).FirstOrDefaultAsync();

        public async Task SubmitAssignmentAsync(AssignmentSubmission sub) =>
            await _db.AssignmentSubmissions.InsertOneAsync(sub);

        public async Task GradeSubmissionAsync(string id, double marks, string? feedback)
        {
            var update = Builders<AssignmentSubmission>.Update
                .Set(s => s.ObtainedMarks, marks).Set(s => s.Feedback, feedback)
                .Set(s => s.IsGraded, true).Set(s => s.GradedAt, DateTime.UtcNow);
            await _db.AssignmentSubmissions.UpdateOneAsync(s => s.Id == id, update);
        }

        // ── FEES ──────────────────────────────────────────────────────
        public async Task<List<Fee>> GetAllFeesAsync() =>
            await _db.Fees.Find(_ => true).SortByDescending(f => f.CreatedAt).ToListAsync();

        public async Task<List<Fee>> GetFeesByStudentAsync(string studentId) =>
            await _db.Fees.Find(f => f.StudentId == studentId)
                .SortByDescending(f => f.CreatedAt).ToListAsync();

        public async Task<Fee?> GetFeeByIdAsync(string id) =>
            await _db.Fees.Find(f => f.Id == id).FirstOrDefaultAsync();

        public async Task CreateFeeAsync(Fee fee)
        {
            var count = await _db.Fees.CountDocumentsAsync(_ => true);
            fee.FeeId = $"FEE-{DateTime.Now.Year}-{(count + 1):D4}";
            await _db.Fees.InsertOneAsync(fee);
        }

        public async Task UpdateFeeStatusAsync(string id, string status)
        {
            var update = Builders<Fee>.Update
                .Set(f => f.Status, status)
                .Set(f => f.PaidAt, status == "Paid" ? DateTime.UtcNow : (DateTime?)null);
            await _db.Fees.UpdateOneAsync(f => f.Id == id, update);
        }

        public async Task SubmitPaymentProofAsync(string feeId, string proofPath, string proofName)
        {
            var update = Builders<Fee>.Update
                .Set(f => f.PaymentProofPath, proofPath)
                .Set(f => f.PaymentProofName, proofName)
                .Set(f => f.ProofSubmittedAt, DateTime.UtcNow)
                .Set(f => f.Status, "Pending Verification");
            await _db.Fees.UpdateOneAsync(f => f.Id == feeId, update);
        }

        public async Task DeleteFeeAsync(string id) =>
            await _db.Fees.DeleteOneAsync(f => f.Id == id);

        // ── FEE REQUESTS ──────────────────────────────────────────────
        public async Task<List<FeeRequest>> GetAllFeeRequestsAsync() =>
            await _db.FeeRequests.Find(_ => true).SortByDescending(r => r.CreatedAt).ToListAsync();

        public async Task<List<FeeRequest>> GetPendingFeeRequestsAsync() =>
            await _db.FeeRequests.Find(r => r.Status == "Pending")
                .SortByDescending(r => r.CreatedAt).ToListAsync();

        public async Task<FeeRequest?> GetFeeRequestByStudentAsync(string studentId) =>
            await _db.FeeRequests.Find(r => r.StudentId == studentId && r.Status == "Pending")
                .FirstOrDefaultAsync();

        public async Task CreateFeeRequestAsync(FeeRequest req) =>
            await _db.FeeRequests.InsertOneAsync(req);

        public async Task MarkFeeRequestDoneAsync(string id)
        {
            var update = Builders<FeeRequest>.Update.Set(r => r.Status, "Done");
            await _db.FeeRequests.UpdateOneAsync(r => r.Id == id, update);
        }

        public async Task<long> CountPendingFeeRequestsAsync() =>
            await _db.FeeRequests.CountDocumentsAsync(r => r.Status == "Pending");

        // ── NOTIFICATIONS ─────────────────────────────────────────────
        public async Task<List<Notification>> GetNotificationsForRoleAsync(string role) =>
            await _db.Notifications.Find(n => n.TargetRole == "all" || n.TargetRole == role)
                .SortByDescending(n => n.CreatedAt).ToListAsync();

        public async Task<List<Notification>> GetAllNotificationsAsync() =>
            await _db.Notifications.Find(_ => true).SortByDescending(n => n.CreatedAt).ToListAsync();

        public async Task CreateNotificationAsync(Notification n) =>
            await _db.Notifications.InsertOneAsync(n);

        public async Task DeleteNotificationAsync(string id) =>
            await _db.Notifications.DeleteOneAsync(n => n.Id == id);

        // ── COMPLAINTS ────────────────────────────────────────────────
        public async Task<List<Complaint>> GetAllComplaintsAsync() =>
            await _db.Complaints.Find(_ => true).SortByDescending(c => c.CreatedAt).ToListAsync();

        public async Task<List<Complaint>> GetComplaintsByUserAsync(string userId) =>
            await _db.Complaints.Find(c => c.UserId == userId)
                .SortByDescending(c => c.CreatedAt).ToListAsync();

        public async Task CreateComplaintAsync(Complaint c) => await _db.Complaints.InsertOneAsync(c);

        public async Task ResolveComplaintAsync(string id, string reply)
        {
            var update = Builders<Complaint>.Update
                .Set(c => c.Status, "resolved").Set(c => c.AdminReply, reply)
                .Set(c => c.ResolvedAt, DateTime.UtcNow);
            await _db.Complaints.UpdateOneAsync(c => c.Id == id, update);
        }

        // ── CHAT ──────────────────────────────────────────────────────
        public async Task<List<ChatQuery>> GetChatHistoryByUserAsync(string userId) =>
            await _db.ChatQueries.Find(q => q.UserId == userId)
                .SortByDescending(q => q.CreatedAt).Limit(50).ToListAsync();

        public async Task<List<ChatQuery>> GetAllChatQueriesAsync() =>
            await _db.ChatQueries.Find(_ => true)
                .SortByDescending(q => q.CreatedAt).Limit(100).ToListAsync();

        public async Task SaveChatQueryAsync(ChatQuery q) => await _db.ChatQueries.InsertOneAsync(q);

        public async Task<long> CountTodayChatQueriesAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _db.ChatQueries.CountDocumentsAsync(q => q.CreatedAt >= today);
        }

        // ── FAQs ──────────────────────────────────────────────────────
        public async Task<List<FAQ>> GetActiveFaqsAsync() =>
            await _db.Faqs.Find(f => f.IsActive).SortBy(f => f.Category).ToListAsync();

        public async Task<List<FAQ>> GetAllFaqsAsync() =>
            await _db.Faqs.Find(_ => true).SortBy(f => f.Category).ToListAsync();

        public async Task<FAQ?> GetFaqByIdAsync(string id) =>
            await _db.Faqs.Find(f => f.Id == id).FirstOrDefaultAsync();

        public async Task CreateFaqAsync(FAQ faq) => await _db.Faqs.InsertOneAsync(faq);

        public async Task UpdateFaqAsync(string id, FAQ faq) =>
            await _db.Faqs.ReplaceOneAsync(f => f.Id == id, faq);

        public async Task DeleteFaqAsync(string id) =>
            await _db.Faqs.DeleteOneAsync(f => f.Id == id);

        public async Task<FAQ?> FindFaqByKeywordAsync(string question)
        {
            var faqs = await GetActiveFaqsAsync();
            var q = question.ToLower();
            FAQ? bestMatch = null;
            int bestScore = 0;

            foreach (var faq in faqs)
            {
                int score = 0;
                if (!string.IsNullOrWhiteSpace(faq.Keywords))
                {
                    var kws = faq.Keywords.ToLower().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var kw in kws)
                        if (q.Contains(kw)) score += 2;
                }
                var qWords = faq.Question.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var w in qWords)
                    if (w.Length > 3 && q.Contains(w)) score += 1;

                if (score > bestScore) { bestScore = score; bestMatch = faq; }
            }
            return bestScore >= 2 ? bestMatch : null;
        }
    }
}