using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class Grade
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

        // Mid Term: out of 30
        [BsonElement("midMarks")]
        public double MidMarks { get; set; } = 0;

        [BsonElement("midTotal")]
        public int MidTotal { get; set; } = 30;

        // Final Term: out of 50
        [BsonElement("finalMarks")]
        public double FinalMarks { get; set; } = 0;

        [BsonElement("finalTotal")]
        public int FinalTotal { get; set; } = 50;

        // Assignments: out of 20
        [BsonElement("assignmentMarks")]
        public double AssignmentMarks { get; set; } = 0;

        [BsonElement("assignmentTotal")]
        public int AssignmentTotal { get; set; } = 20;

        // Total = Mid(30) + Final(50) + Assignment(20) = 100
        [BsonElement("totalMarks")]
        public double TotalMarks { get; set; } = 0;

        [BsonElement("maxTotal")]
        public int MaxTotal { get; set; } = 100;

        [BsonElement("grade")]
        public string GradeLetter { get; set; } = "N/A";

        [BsonElement("gpa")]
        public double GPA { get; set; } = 0;

        [BsonElement("semesterNo")]
        public int SemesterNo { get; set; }

        [BsonElement("remarks")]
        public string? Remarks { get; set; }

        // ?? ?? ??? ???? ??????? ?? ???? ?? ??? ???? ?? ??? ??? ??? ??? ?? ??
        [BsonElement("isReleased")]
        public bool IsReleased { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // --- Static helpers ---

        public static string CalculateGrade(double total, double outOf = 100)
        {
            var pct = total / outOf * 100;
            return pct switch
            {
                >= 90 => "A+",
                >= 85 => "A",
                >= 80 => "A-",
                >= 75 => "B+",
                >= 70 => "B",
                >= 65 => "B-",
                >= 60 => "C+",
                >= 55 => "C",
                >= 50 => "C-",
                >= 45 => "D+",
                >= 40 => "D",
                _ => "F"
            };
        }

        public static double CalculateGPA(string grade)
        {
            return grade switch
            {
                "A+" => 4.0,
                "A" => 4.0,
                "A-" => 3.7,
                "B+" => 3.3,
                "B" => 3.0,
                "B-" => 2.7,
                "C+" => 2.3,
                "C" => 2.0,
                "C-" => 1.7,
                "D+" => 1.3,
                "D" => 1.0,
                _ => 0.0
            };
        }
    }
}