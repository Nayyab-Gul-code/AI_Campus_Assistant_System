using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class Lecture
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

        [BsonElement("fileName")]
        public string FileName { get; set; } = string.Empty;

        [BsonElement("filePath")]
        public string FilePath { get; set; } = string.Empty;

        [BsonElement("fileType")]
        public string FileType { get; set; } = string.Empty; // pdf, docx, pptx, xlsx

        [BsonElement("fileSize")]
        public long FileSize { get; set; } = 0;

        [BsonElement("program")]
        public string Program { get; set; } = string.Empty;

        [BsonElement("semesterNo")]
        public int SemesterNo { get; set; }

        [BsonElement("weekNo")]
        public int WeekNo { get; set; } = 1;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
