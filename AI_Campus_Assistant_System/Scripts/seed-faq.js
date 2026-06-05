// seed-faq.js — Run manually: mongosh AICampusDb --file seed-faq.js
db = db.getSiblingDB('AICampusDb');

db.Faqs.deleteMany({});

db.Faqs.insertMany([
  { question: "How do I check my grades?",              answer: "Go to Student Dashboard → Grades section to view all your subject grades.", category: "academics", isActive: true, createdAt: new Date() },
  { question: "How to submit an assignment?",           answer: "Navigate to Assignments page, click on the assignment title and fill in your submission.", category: "academics", isActive: true, createdAt: new Date() },
  { question: "What is minimum attendance required?",   answer: "Minimum 75% attendance is required to appear in final exams.", category: "attendance", isActive: true, createdAt: new Date() },
  { question: "How do I check my timetable?",          answer: "Go to Student Dashboard → Timetable to see your weekly class schedule.", category: "schedule", isActive: true, createdAt: new Date() },
  { question: "How to file a complaint?",               answer: "Go to Complaints section and click 'New Complaint' to submit your issue to admin.", category: "support", isActive: true, createdAt: new Date() },
  { question: "How to contact my teacher?",             answer: "Go to your course page to find teacher contact information and office hours.", category: "support", isActive: true, createdAt: new Date() },
  { question: "When are mid-term exams?",               answer: "Check the Notifications section for exam schedules announced by admin.", category: "academics", isActive: true, createdAt: new Date() },
  { question: "How to reset my password?",              answer: "Go to Profile → Change Password section to update your password.", category: "account", isActive: true, createdAt: new Date() },
  { question: "How many courses can I enroll in?",      answer: "Typically 5-6 courses per semester based on your program requirements.", category: "academics", isActive: true, createdAt: new Date() },
  { question: "What grading scale is used?",            answer: "A (85-100), B+ (80-84), B (75-79), C+ (70-74), C (65-69), D (60-64), F (below 60).", category: "academics", isActive: true, createdAt: new Date() },
]);

print('✅ FAQs seeded: ' + db.Faqs.countDocuments() + ' records');
