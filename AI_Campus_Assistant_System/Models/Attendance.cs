using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class Attendance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [BsonElement("studentName")]
        public string StudentName { get; set; } = string.Empty;

        [BsonElement("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [BsonElement("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [BsonElement("teacherId")]
        public string TeacherId { get; set; } = string.Empty;

        [BsonElement("date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [BsonElement("status")]
        public string Status { get; set; } = "present"; // present, absent, late

        [BsonElement("remarks")]
        public string? Remarks { get; set; }
    }
}
