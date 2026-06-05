using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Campus_Assistant.Models
{
    public class Timetable
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("courseId")]
        public string CourseId { get; set; } = string.Empty;

        [BsonElement("courseName")]
        public string CourseName { get; set; } = string.Empty;

        [BsonElement("courseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [BsonElement("teacherId")]
        public string? TeacherId { get; set; }

        [BsonElement("teacherName")]
        public string TeacherName { get; set; } = string.Empty;

        // Monday to Friday only
        [BsonElement("dayOfWeek")]
        public string DayOfWeek { get; set; } = string.Empty;

        // TimeSlot: e.g. "08:00 - 08:50"
        [BsonElement("timeSlot")]
        public string TimeSlot { get; set; } = string.Empty;

        [BsonElement("startTime")]
        public string StartTime { get; set; } = string.Empty; // "08:00"

        [BsonElement("endTime")]
        public string EndTime { get; set; } = string.Empty;   // "08:50"

        [BsonElement("room")]
        public string Room { get; set; } = string.Empty;

        [BsonElement("program")]
        public string Program { get; set; } = string.Empty; // BSCS

        [BsonElement("semesterNo")]
        public int SemesterNo { get; set; } = 1;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Helper: Defines available time slots
    public static class TimeSlots
    {
        // Valid Mon–Thu slots (15-min break between each 50-min class)
        public static readonly string[] MondayToThursday = new[]
        {
            "08:00-08:50", "09:05-09:55", "10:10-11:00",
            "11:15-12:05", "12:20-13:10", "13:25-14:15", "14:30-15:00"
        };

        // Friday: 8AM-1PM classes, 1PM-2PM Juma break, 2PM-3PM optional
        public static readonly string[] Friday = new[]
        {
            "08:00-08:50", "09:05-09:55", "10:10-11:00", "11:15-12:05", "12:20-13:00",
            // 13:00-14:00 = JUMA PRAYER BREAK (no classes)
            "14:00-14:50", "14:55-15:00"
        };

        public static readonly string[] ValidDays = new[]
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday"
        };

        // Returns true if the slot is allowed on the given day
        public static bool IsValidSlot(string day, string startTime)
        {
            if (day == "Saturday" || day == "Sunday") return false;

            // Friday: block 13:00–14:00 (Juma)
            if (day == "Friday")
            {
                var start = TimeSpan.Parse(startTime);
                if (start >= TimeSpan.FromHours(13) && start < TimeSpan.FromHours(14))
                    return false;
            }

            // All days: 8AM – 3PM only
            var t = TimeSpan.Parse(startTime);
            return t >= TimeSpan.FromHours(8) && t < TimeSpan.FromHours(15);
        }
    }
}
