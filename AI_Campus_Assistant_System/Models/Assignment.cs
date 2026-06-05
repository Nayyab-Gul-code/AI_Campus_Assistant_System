using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class Assignment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [BsonElement("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [BsonElement("teacherId")]
        public string TeacherId { get; set; } = string.Empty;

        [BsonElement("teacherName")]
        public string TeacherName { get; set; } = string.Empty;

        [BsonElement("dueDate")]
        public DateTime DueDate { get; set; }

        [BsonElement("totalMarks")]
        public int TotalMarks { get; set; } = 20;

        [BsonElement("program")]
        public string Program { get; set; } = string.Empty;

        [BsonElement("semesterNo")]
        public int SemesterNo { get; set; }

        // File attachment by teacher (question paper)
        [BsonElement("fileName")]
        public string? FileName { get; set; }

        [BsonElement("filePath")]
        public string? FilePath { get; set; }

        [BsonElement("fileType")]
        public string? FileType { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AssignmentSubmission
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("assignmentId")]
        public string AssignmentId { get; set; } = string.Empty;

        [BsonElement("assignmentTitle")]
        public string AssignmentTitle { get; set; } = string.Empty;

        [BsonElement("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [BsonElement("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [BsonElement("studentName")]
        public string StudentName { get; set; } = string.Empty;

        [BsonElement("submittedAt")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("content")]
        public string? Content { get; set; }

        // Student file submission
        [BsonElement("fileName")]
        public string? FileName { get; set; }

        [BsonElement("filePath")]
        public string? FilePath { get; set; }

        [BsonElement("fileType")]
        public string? FileType { get; set; }

        [BsonElement("fileSize")]
        public long FileSize { get; set; } = 0;

        // Grading by teacher
        [BsonElement("obtainedMarks")]
        public double? ObtainedMarks { get; set; }

        [BsonElement("totalMarks")]
        public int TotalMarks { get; set; } = 20;

        [BsonElement("feedback")]
        public string? Feedback { get; set; }

        [BsonElement("isGraded")]
        public bool IsGraded { get; set; } = false;

        [BsonElement("gradedAt")]
        public DateTime? GradedAt { get; set; }
    }
}