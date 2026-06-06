using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class Semester
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty; // Fall 2025, Spring 2026

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        public DateTime EndDate { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
