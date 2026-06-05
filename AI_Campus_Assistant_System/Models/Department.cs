using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class Department
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty; // BSCS, BSCE, MS, MBA

        [BsonElement("code")]
        public string Code { get; set; } = string.Empty; // CS, CE, MS, MB

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("headName")]
        public string? HeadName { get; set; }

        [BsonElement("totalSemesters")]
        public int TotalSemesters { get; set; } = 8; // 8 for BS, 4 for MS/MBA

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
