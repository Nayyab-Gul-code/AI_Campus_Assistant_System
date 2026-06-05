// MongoDB initialization script
db = db.getSiblingDB('AICampusDb');

// Create collections with indexes
db.createCollection('Users');
db.Users.createIndex({ email: 1 }, { unique: true });

db.createCollection('Courses');
db.Courses.createIndex({ teacherId: 1 });

db.createCollection('Departments');
db.createCollection('Semesters');
db.createCollection('Timetables');
db.Timetables.createIndex({ program: 1, semesterNo: 1 });

db.createCollection('Attendances');
db.Attendances.createIndex({ studentId: 1, courseId: 1 });

db.createCollection('Assignments');
db.createCollection('AssignmentSubmissions');
db.AssignmentSubmissions.createIndex({ assignmentId: 1, studentId: 1 });

db.createCollection('Notifications');
db.createCollection('ChatQueries');
db.createCollection('Faqs');
db.createCollection('Complaints');
db.createCollection('Grades');
db.Grades.createIndex({ studentId: 1, courseId: 1 });

print('✅ MongoDB AICampusDb initialized with all collections and indexes.');
