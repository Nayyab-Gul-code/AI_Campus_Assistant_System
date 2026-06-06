// Data/MongoDbContext.cs
using MongoDB.Driver;
using AI_Campus_Assistant.Models;
using Microsoft.Extensions.Options;

namespace AI_Campus_Assistant.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
            CreateIndexes();
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Course> Courses => _database.GetCollection<Course>("Courses");
        public IMongoCollection<Department> Departments => _database.GetCollection<Department>("Departments");
        public IMongoCollection<Semester> Semesters => _database.GetCollection<Semester>("Semesters");
        public IMongoCollection<Timetable> Timetables => _database.GetCollection<Timetable>("Timetables");
        public IMongoCollection<Attendance> Attendances => _database.GetCollection<Attendance>("Attendances");
        public IMongoCollection<Assignment> Assignments => _database.GetCollection<Assignment>("Assignments");
        public IMongoCollection<AssignmentSubmission> AssignmentSubmissions => _database.GetCollection<AssignmentSubmission>("AssignmentSubmissions");
        public IMongoCollection<Lecture> Lectures => _database.GetCollection<Lecture>("Lectures");
        public IMongoCollection<Fee> Fees => _database.GetCollection<Fee>("fees");
        public IMongoCollection<FeeRequest> FeeRequests => _database.GetCollection<FeeRequest>("feeRequests");
        public IMongoCollection<Grade> Grades => _database.GetCollection<Grade>("Grades");
        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");
        public IMongoCollection<ChatQuery> ChatQueries => _database.GetCollection<ChatQuery>("ChatQueries");
        public IMongoCollection<FAQ> Faqs => _database.GetCollection<FAQ>("Faqs");
        public IMongoCollection<Complaint> Complaints => _database.GetCollection<Complaint>("Complaints");

        private void CreateIndexes()
        {
            Users.Indexes.CreateOne(new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }));

            Courses.Indexes.CreateOne(new CreateIndexModel<Course>(
                Builders<Course>.IndexKeys.Ascending(c => c.TeacherId)));

            Attendances.Indexes.CreateOne(new CreateIndexModel<Attendance>(
                Builders<Attendance>.IndexKeys.Ascending(a => a.StudentId).Ascending(a => a.CourseId)));

            Grades.Indexes.CreateOne(new CreateIndexModel<Grade>(
                Builders<Grade>.IndexKeys.Ascending(g => g.StudentId).Ascending(g => g.CourseId)));

            Lectures.Indexes.CreateOne(new CreateIndexModel<Lecture>(
                Builders<Lecture>.IndexKeys.Ascending(l => l.CourseId).Ascending(l => l.Program)));

            Fees.Indexes.CreateOne(new CreateIndexModel<Fee>(
                Builders<Fee>.IndexKeys.Ascending(f => f.FeeId),
                new CreateIndexOptions { Unique = true, Sparse = true }));
        }
    }
}