using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace YourProjectName.Models
{
    public class ResultCard
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("StudentId")]
        public string StudentId { get; set; }

        [BsonElement("StudentName")]
        public string StudentName { get; set; }

        [BsonElement("RollNumber")]
        public string RollNumber { get; set; }

        [BsonElement("Semester")]
        public string Semester { get; set; }

        [BsonElement("Subjects")]
        public List<SubjectGrade> Subjects { get; set; } = new List<SubjectGrade>();

        [BsonElement("Remarks")]
        public string Remarks { get; set; }

        [BsonElement("IsGenerated")]
        public bool IsGenerated { get; set; } = false;
    }

    public class SubjectGrade
    {
        [BsonElement("SubjectId")]
        public string SubjectId { get; set; }

        [BsonElement("SubjectName")]
        public string SubjectName { get; set; }

        [BsonElement("Grade")]
        public string Grade { get; set; }
    }
}