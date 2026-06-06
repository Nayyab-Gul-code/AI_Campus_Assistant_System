using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class FeeRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [BsonElement("studentName")]
        public string StudentName { get; set; } = string.Empty;

        [BsonElement("program")]
        public string Program { get; set; } = string.Empty;

        [BsonElement("semesterNo")]
        public int SemesterNo { get; set; }

        [BsonElement("wantsTransport")]
        public bool WantsTransport { get; set; } = false;

        [BsonElement("wantsHostel")]
        public bool WantsHostel { get; set; } = false;

        [BsonElement("studentNote")]
        public string? StudentNote { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending / Done

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}