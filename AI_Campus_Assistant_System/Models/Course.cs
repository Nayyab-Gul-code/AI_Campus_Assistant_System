using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
         public string Title { get; set; } = string.Empty;

        [BsonElement("code")]
        public string Code { get; set; } = string.Empty; // CS-301
        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("departmentId")]
        public string? DepartmentId { get; set; }

        [BsonElement("departmentName")]
        public string? DepartmentName { get; set; }

        [BsonElement("semesterNo")]
        public int SemesterNo { get; set; } = 1;

        [BsonElement("program")]
        public string Program { get; set; } = "BSCS"; // BSCS, MS, MBA

        [BsonElement("teacherId")]
        public string? TeacherId { get; set; }

        [BsonElement("teacherName")]
        public string? TeacherName { get; set; }

        [BsonElement("credits")]
        public int Credits { get; set; } = 3;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
