using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = "student"; // admin, student, teacher

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("program")]
        public string? Program { get; set; } // BSCS, MS, MBA etc

        [BsonElement("departmentId")]
        public string? DepartmentId { get; set; }

        [BsonElement("semesterNo")]
        public int SemesterNo { get; set; } = 1;

        [BsonElement("designation")]
        public string? Designation { get; set; } // For teachers

        [BsonElement("bio")]
        public string? Bio { get; set; }

        [BsonElement("profilePic")]
        public string? ProfilePic { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastLogin")]
        public DateTime? LastLogin { get; set; }
    }
}
