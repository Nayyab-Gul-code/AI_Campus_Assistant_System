# 🎓 AI Campus Assistant System

A full-stack **Smart University Campus Management System** built with **ASP.NET Core 8 MVC**, **MongoDB**, and **Google Gemini API**. The platform automates academic and administrative operations for a university, supporting three roles: **Admin**, **Teacher**, and **Student**.

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [User Roles & Modules](#-user-roles--modules)
- [Data Models](#-data-models)
- [AI Integration](#-ai-integration)
- [File Uploads](#-file-uploads)
- [Security](#-security)
- [Database Indexes](#-database-indexes)
- [Default Login Credentials](#-default-login-credentials)
- [Screenshots Overview](#-screenshots-overview)
- [Known Limitations](#-known-limitations)
- [Future Enhancements](#-future-enhancements)
- [Conclusion](#-conclusion)

---

## ✨ Features

### 🔐 Authentication & Authorization
- Cookie-based authentication with **8-hour sliding session**
- Role-based access control: `admin`, `teacher`, `student`
- BCrypt password hashing (BCrypt.Net-Next v4.0.3)
- CSRF protection on all POST actions

### 👨‍💼 Admin Panel
- Real-time dashboard with system-wide statistics
- Full user management (Create / Edit / Delete users)
- Department and course management
- Timetable creation with built-in time conflict validation
- Fee challan generation from student requests or manually
- Broadcast notifications (all / students / teachers)
- Complaint resolution with written admin replies
- AI Chat Monitor — view all queries from all users

### 👨‍🏫 Teacher Panel
- Dashboard with course, assignment, and lecture summaries
- Upload lecture materials (PDF, DOCX, PPTX, XLSX)
- Create assignments with optional question paper attachment
- Mark attendance with real-time AJAX student loading
- View and grade student submissions with feedback
- Enter Mid-Term, Final, and Assignment grades
- AI-powered teaching improvement suggestions (Gemini Pro)
- Personal timetable and notification feed

### 👨‍🎓 Student Panel
- Dashboard: grades, attendance %, unpaid fees, upcoming assignments
- Download lectures and assignment files
- Submit assignments (text or file) with duplicate prevention
- View grades with letter grade and GPA per course
- Attendance tracker (present / absent / late breakdown)
- Fee challan view + payment proof upload (up to 5 MB)
- Submit fee preferences (transport, hostel request)
- **AI Chat Assistant** powered by Google Gemini Pro (English & Urdu)
- Complaints with admin response tracking
- Profile management and password change

---

## 🛠 Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 MVC (C#) |
| Database | MongoDB with MongoDB.Driver 2.28.0 |
| AI | Google Gemini Pro REST API |
| Authentication | Cookie Auth + BCrypt.Net-Next |
| Frontend | Bootstrap 5, jQuery, Razor Views |
| File Storage | Server-side disk (wwwroot/uploads/) |
| Target Runtime | .NET 8.0 |

---

## 📁 Project Structure

```
AI_Campus_Assistant_System/
│
├── Controllers/
│   ├── AdminController.cs        # Admin-only routes [Authorize(Roles="admin")]
│   ├── TeacherController.cs      # Teacher-only routes [Authorize(Roles="teacher")]
│   ├── StudentController.cs      # Student-only routes [Authorize(Roles="student")]
│   └── AuthController.cs         # Login, Register, Logout (public)
│
├── Models/
│   ├── User.cs                   # User (admin/teacher/student)
│   ├── Course.cs                 # Course with program, semester, teacher
│   ├── Department.cs             # Academic department
│   ├── Timetable.cs              # Class schedule + TimeSlots helper
│   ├── Attendance.cs             # Per-student per-course attendance
│   ├── Grade.cs                  # Mid + Final + Assignment grades with GPA
│   ├── Assignment.cs             # Assignment + AssignmentSubmission
│   ├── Lecture.cs                # Uploaded lecture materials
│   ├── Fee.cs                    # Fee challan with payment proof
│   ├── FeeRequest.cs             # Student fee preference request
│   ├── Notification.cs           # Role-targeted notifications
│   ├── Complaint.cs              # User complaints with admin resolution
│   ├── ChatQuery.cs              # AI chat history
│   ├── FAQ.cs                    # Pre-seeded frequently asked questions
│   └── MongoDbSettings.cs        # Configuration model
│
├── Services/
│   ├── MongoDbService.cs         # All data access operations (CRUD)
│   ├── GeminiAiService.cs        # Google Gemini Pro API integration
│   ├── AuthService.cs            # Login, register, password management
│   └── SeedDataService.cs        # Auto-seeds admin user on startup
│
├── Data/
│   └── MongoDbContext.cs         # MongoDB collections + index creation
│
├── Scripts/
│   ├── mongo-init.js             # MongoDB initialization script (Docker)
│   └── seed-faq.js               # FAQ seed data script
│
├── wwwroot/
│   ├── css/site.css              # Custom styles
│   ├── js/site.js                # Custom scripts
│   ├── uploads/                  # Runtime-created upload directories
│   └── lib/                      # Bootstrap 5, jQuery, validation libs
│
├── Program.cs                    # App bootstrap, DI registration, middleware
├── appsettings.json              # MongoDB & Gemini API configuration
├── appsettings.Development.json  # Dev overrides
├── .dockerignore                 # Docker build exclusions
└── AI_Campus_Assistant_System.csproj
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (local) or [MongoDB Atlas](https://www.mongodb.com/atlas) (cloud)
- Google Gemini Pro API Key — get it free at [Google AI Studio](https://aistudio.google.com/)

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/your-username/AI_Campus_Assistant_System..git
cd AI_Campus_Assistant_System/AI_Campus_Assistant_System
```

**2. Configure the application**

Open `appsettings.json` and update the values:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "AICampusDb"
  },
  "GeminiApiKey": "YOUR_GEMINI_API_KEY_HERE"
}
```

**3. Restore dependencies**
```bash
dotnet restore
```

**4. Run the application**
```bash
dotnet run
```

**5. Open in browser**
```
https://localhost:5001
```

The application will **auto-seed** an admin account on first run via `SeedDataService`.

---

### 🐳 Running with Docker

The project includes a `.dockerignore` for Docker deployments.

```bash
# Build image
docker build -t ai-campus-assistant .

# Run with MongoDB
docker run -p 8080:80 \
  -e MongoDbSettings__ConnectionString="mongodb://mongo:27017" \
  -e MongoDbSettings__DatabaseName="AICampusDb" \
  -e GeminiApiKey="YOUR_KEY_HERE" \
  ai-campus-assistant
```

Use `docker-compose` with the `mongo-init.js` script to initialize MongoDB automatically.

---

## ⚙️ Configuration

| Key | Description | Example |
|-----|-------------|---------|
| `MongoDbSettings:ConnectionString` | MongoDB connection URI | `mongodb://localhost:27017` |
| `MongoDbSettings:DatabaseName` | MongoDB database name | `AICampusDb` |
| `GeminiApiKey` | Google Gemini Pro API key | `AIza...` |

> **Tip:** For production, store the Gemini API key in environment variables or .NET User Secrets rather than `appsettings.json`.

```bash
# Using .NET User Secrets (development)
dotnet user-secrets set "GeminiApiKey" "YOUR_KEY_HERE"
```

---

## 👥 User Roles & Modules

### Admin
| Module | Features |
|--------|----------|
| Dashboard | Student count, teacher count, course count, pending complaints, fee stats, AI query count |
| Users | Create / edit / delete users; filter by role; search by name or email |
| Departments | Full CRUD for academic departments |
| Courses | Create / assign courses to departments and teachers |
| Timetable | Schedule classes Mon–Fri, 8AM–3PM; Friday Juma break (1–2 PM) blocked |
| Fees | Generate challans from student requests or manually; mark paid/unpaid |
| Notifications | Broadcast to all, students, or teachers |
| Complaints | View all complaints; resolve with written reply |
| AI Monitor | View all student chat queries and today's count |

### Teacher
| Module | Features |
|--------|----------|
| Dashboard | My courses, assignment count, lecture count, timetable |
| Lectures | Upload files, set week number, delete; students can download |
| Assignments | Create with optional question paper; grade submissions |
| Attendance | AJAX-loaded student list; mark present/absent/late; view reports |
| Grades | Enter Mid/Final/Assignment marks; auto GPA and letter grade |
| AI Suggestions | Gemini AI analyzes your course stats and suggests improvements |
| Complaints | Submit and track complaints |
| Profile | Update info and change password |

### Student
| Module | Features |
|--------|----------|
| Dashboard | GPA summary, attendance %, unpaid fees, upcoming assignments |
| Lectures | View and download lecture files by program and semester |
| Assignments | Submit (text or file); track submission and graded feedback |
| Grades | Per-course breakdown: Mid + Final + Assignment → Total, Grade, GPA |
| Attendance | Present / Absent / Late with overall percentage |
| Fees | View challans, submit fee request, upload payment proof |
| AI Chat | Natural language queries answered by Gemini Pro (English & Urdu) |
| Notifications | Admin broadcasts targeting students |
| Complaints | Submit complaints; view admin resolution |
| Profile | Update info, change password |

---

## 🗃️ Data Models

### Grade Calculation

```
Total = MidMarks (30) + FinalMarks (50) + AssignmentMarks (20) = 100
```

| Percentage | Letter Grade | GPA |
|-----------|-------------|-----|
| ≥ 90% | A+ | 4.0 |
| ≥ 85% | A | 4.0 |
| ≥ 80% | A- | 3.7 |
| ≥ 75% | B+ | 3.3 |
| ≥ 70% | B | 3.0 |
| ≥ 65% | B- | 2.7 |
| ≥ 60% | C+ | 2.3 |
| ≥ 55% | C | 2.0 |
| ≥ 50% | C- | 1.7 |
| ≥ 45% | D+ | 1.3 |
| ≥ 40% | D | 1.0 |
| < 40% | F | 0.0 |

### Fee Structure (Default)

| Component | Amount (PKR) | Condition |
|-----------|-------------|-----------|
| Tuition Fee | 25,000 | Always |
| Library Fee | 1,000 | Always |
| Transport Fee | 3,000 | If `HasTransport = true` |
| Hostel Fee | 8,000 | If `HasHostel = true` |

Fee ID format: `FEE-YYYY-NNNN` (e.g., `FEE-2026-0001`)

### Timetable Rules

- Valid days: **Monday – Friday** only
- Valid hours: **8:00 AM – 3:00 PM**
- Friday **1:00 PM – 2:00 PM** is blocked (Juma Prayer)
- Saturday and Sunday are rejected

---

## 🤖 AI Integration

The `GeminiAiService` communicates with Google's Gemini Pro REST API.

**Endpoint:** `https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent`

### Student Chat
- AI persona: university campus assistant
- Responds in the user's language (English or Urdu)
- Chat history stored in MongoDB (last 50 per user)

### Teacher Suggestions
- Input: course name, total students, average attendance %, average grade
- Output: 3–4 numbered teaching improvement tips (max 150 words)

### Error Handling

| Situation | Response |
|-----------|----------|
| API key not set | `"AI service not configured. Please add GeminiApiKey..."` |
| Non-2xx response | `"AI service error: {StatusCode}. Please check your API key."` |
| Network exception | `"AI Error: {exception message}"` |

---

## 📂 File Uploads

| Type | Storage Path | Allowed Formats | Limit |
|------|-------------|----------------|-------|
| Lecture materials | `/wwwroot/uploads/lectures/` | PDF, DOCX, DOC, PPTX, PPT, XLSX, XLS | — |
| Assignment papers | `/wwwroot/uploads/assignments/` | PDF, DOCX, DOC, PPTX, PPT, XLSX, XLS | — |
| Student submissions | `/wwwroot/uploads/submissions/` | PDF, DOCX, DOC, PPTX, PPT, XLSX, XLS | — |
| Payment proofs | `/wwwroot/uploads/payment-proofs/` | PDF, JPG, JPEG, PNG | **5 MB** |

All uploaded files are renamed to `{GUID}.{ext}` before saving to prevent conflicts. Original filenames are preserved in the database for download response headers. Deleting a lecture or assignment also removes the physical file from disk.

---

## 🔒 Security

| Concern | Implementation |
|---------|---------------|
| Password storage | BCrypt hash — plaintext never stored |
| Session security | HttpOnly cookies, 8-hour expiry, sliding renewal |
| CSRF protection | `[ValidateAntiForgeryToken]` on all POST actions |
| Role enforcement | `[Authorize(Roles = "...")]` on all role controllers |
| File extension validation | Whitelist approach on every upload endpoint |
| Email normalization | Stored as lowercase + trimmed |
| File naming | GUID-based names prevent path traversal |
| Injection attacks | MongoDB typed LINQ — no raw query strings |

---

## 🗄️ Database Indexes

The following MongoDB indexes are created automatically on startup via `MongoDbContext.CreateIndexes()`:

| Collection | Index Fields | Type |
|-----------|-------------|------|
| Users | `Email` | Unique |
| Courses | `TeacherId` | Regular |
| Attendances | `StudentId + CourseId` | Compound |
| Grades | `StudentId + CourseId` | Compound |
| Lectures | `CourseId + Program` | Compound |
| Fees | `FeeId` | Unique, Sparse |

---

## 🔑 Default Login Credentials

The system auto-seeds an admin account on first startup:

| Field | Value |
|-------|-------|
| Email | `admin@campus.com` |
| Password | `admin123` |
| Role | admin |

> ⚠️ **Change the default admin password immediately after first login in a production environment.**

---

## 📸 Screenshots Overview

| Page | Description |
|------|-------------|
| `/Auth/Login` | Clean login form, redirect by role |
| `/Admin/Dashboard` | Stats cards, recent users, notifications |
| `/Admin/Fees` | Fee table with filters, pending requests panel |
| `/Teacher/Attendance` | AJAX course → students → mark attendance |
| `/Teacher/GradeStudents` | Per-student grade entry with auto GPA |
| `/Student/Chat` | Gemini AI chat with FAQ panel |
| `/Student/Fees` | Challan list, fee request form, proof upload |
| `/Student/Grades` | Course grades table with GPA and letter grades |

---

## ⚠️ Known Limitations

- Lecture and assignment file uploads have no size cap (only payment proofs are limited to 5 MB)
- Self-registration allows any role — no admin approval flow at registration
- AI chat history capped at last 50 queries; no pagination for older history
- No real-time notifications (requires page refresh; SignalR not yet integrated)
- No email verification or email-based password reset
- GPA is per-course only — no cumulative CGPA aggregation across semesters

---

## 🔮 Future Enhancements

- [ ] Real-time notifications via SignalR
- [ ] Email verification and password reset via email
- [ ] PDF fee challan generation and download
- [ ] Cumulative CGPA calculation across semesters
- [ ] Online quiz and examination module
- [ ] Stripe / JazzCash / EasyPaisa online fee payment
- [ ] Mobile-responsive Progressive Web App (PWA)
- [ ] Urdu right-to-left (RTL) full UI support
- [ ] Admin audit log for sensitive actions
- [ ] Two-factor authentication (2FA)
- [ ] Student GPA trend charts and analytics dashboard

---

## 📄 License

This project is developed for academic purposes. Contact the repository owner for usage permissions.

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Commit your changes: `git commit -m "Add: your feature description"`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Open a Pull Request

---

## ✅ Conclusion

**AI Campus Assistant System** is a smart university management platform built with ASP.NET Core 8 MVC, MongoDB, and Google Gemini Pro. It streamlines academic operations, provides AI-powered assistance, ensures secure role-based access, and offers a scalable architecture for students, teachers, and administrators in a single integrated system.


---

> Built with  using ASP.NET Core 8 | MongoDB | Google Gemini AI | Bootstrap 5
