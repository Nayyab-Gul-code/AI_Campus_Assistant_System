# 🎓 AI Campus Assistant System

A full-featured **Smart University Campus Management System** built with **ASP.NET Core 8 (MVC)**, **MongoDB**, and **Groq AI (Llama 3.3 70B)**. It provides role-based dashboards for Admins, Teachers, and Students — plus an integrated AI chatbot that understands both English and Urdu.

---

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Option A — Docker Compose (Recommended)](#option-a--docker-compose-recommended)
  - [Option B — Run Locally (Manual)](#option-b--run-locally-manual)
- [Configuration](#configuration)
- [Default Credentials](#default-credentials)
- [Role-Based Modules](#role-based-modules)
  - [Admin Panel](#admin-panel)
  - [Teacher Portal](#teacher-portal)
  - [Student Portal](#student-portal)
- [Database Schema](#database-schema)
- [AI Integration](#ai-integration)
- [Security Notes](#security-notes)
- [Troubleshooting](#troubleshooting)

---

## ✨ Features

### Core Academic Management
- Role-based access control — **Admin**, **Teacher**, **Student**
- Department and course management
- Semester and timetable scheduling
- Attendance tracking with per-course reports
- Assignment creation, file upload, submission, and AI-powered grading

### Grading & Results
- Grade breakdown: Mid-Term (30) + Final (50) + Assignments (20) = 100 marks
- Auto-calculated letter grades (A+, A, B+, B, C, D, F)
- Printable student result cards (PDF-style view)

### Fee Management
- Admin fee records per student and semester
- Student fee challan generation
- Payment proof upload by students
- Admin verification workflow (Pending → Verified / Rejected)

### AI Features
- **Student AI Chat** — ask academic or campus questions; AI responds in Urdu or English
- **Teacher AI Suggestions** — per-course teaching improvement tips based on attendance and grade analytics
- **Admin AI Monitor** — overview of AI usage statistics and recent chat queries
- **Teacher Content Generator** — generate quiz/assignment questions via AI

### Communication
- Notification system (admin broadcasts to students/teachers)
- Student complaint submission with admin resolution tracking
- FAQ management by admin, surfaced in student chat

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| Database | MongoDB 6+ (via `MongoDB.Driver 3.9`) |
| Authentication | Cookie-based auth (`Microsoft.AspNetCore.Authentication.Cookies`) |
| Password Hashing | BCrypt.Net-Next 4.0.3 |
| AI Backend | Groq API — Llama 3.3 70B Versatile |
| Frontend | Bootstrap 5, jQuery, custom CSS/JS |
| Containerisation | Docker + Docker Compose |
| Target Runtime | .NET 8.0 |

---

## 📁 Project Structure

```
AI_Campus_Assistant_System/
└── AI_Campus_Assistant_System/      ← Main ASP.NET Core project
    ├── Controllers/
    │   ├── AdminController.cs       ← All admin endpoints (~605 lines)
    │   ├── AuthController.cs        ← Login / Logout / Register
    │   ├── StudentController.cs     ← Student portal endpoints
    │   └── TeacherController.cs     ← Teacher portal endpoints
    ├── Data/
    │   ├── MongoDbContext.cs         ← MongoDB collections + indexes
    │   └── SeedData.cs              ← Initial data seeder
    ├── Models/
    │   ├── User.cs                  ← Admin / Teacher / Student
    │   ├── Course.cs, Department.cs, Semester.cs
    │   ├── Assignment.cs            ← Includes AssignmentSubmission
    │   ├── Attendance.cs
    │   ├── Grade.cs                 ← Mid + Final + Assignment marks
    │   ├── Fee.cs, FeeRequest.cs
    │   ├── Timetable.cs, Lecture.cs
    │   ├── ChatQuery.cs, FAQ.cs
    │   ├── Complaint.cs, Notification.cs
    │   └── ResultCard.cs
    ├── Services/
    │   ├── AuthService.cs           ← Login validation, claims principal
    │   ├── MongoDbService.cs        ← All DB read/write operations
    │   ├── GroqAiService.cs         ← Groq API wrapper (Llama 3.3 70B)
    │   └── SeedDataService.cs       ← Calls SeedData on startup
    ├── Views/
    │   ├── Admin/                   ← Dashboard, Users, Courses, Fees, etc.
    │   ├── Teacher/                 ← Dashboard, Grades, AI, Timetable, etc.
    │   ├── Student/                 ├── Dashboard, Chat, Fees, ResultCard, etc.
    │   ├── Auth/                    ← Login, Register
    │   └── Shared/                  ← Layouts (Admin / Teacher / Student)
    ├── Scripts/
    │   ├── mongo-init.js            ← MongoDB init script (for Docker)
    │   └── seed-faq.js             ← FAQ seed script
    ├── wwwroot/
    │   ├── css/site.css, js/site.js
    │   └── uploads/                ← Assignment files, submissions, payment proofs
    ├── Program.cs                   ← App startup + DI + middleware
    ├── appsettings.json             ← MongoDB connection + Groq API key
    ├── Dockerfile
    └── docker-compose.yml
```

---

## ⚙️ Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MongoDB 6+](https://www.mongodb.com/try/download/community) **or** Docker Desktop
- A free [Groq API key](https://console.groq.com) (for AI features)

---

## 🚀 Getting Started

### Option A — Docker Compose (Recommended)

This is the easiest way. Docker will start both the app and MongoDB together.

```bash
# 1. Clone / extract the project
cd AI_Campus_Assistant_System

# 2. Add your Groq API key (see Configuration section below)

# 3. Start everything
docker compose up --build
```

The app will be available at **http://localhost:5000**.

MongoDB runs on port **27017** inside the Docker network.

---

### Option B — Run Locally (Manual)

**1. Start MongoDB**

Make sure MongoDB is running locally on port `27017`, or update the connection string in `appsettings.json`.

**2. Set your Groq API key**

Edit `appsettings.json` (or use .NET User Secrets):

```json
{
  "GroqApiKey": "your_groq_api_key_here"
}
```

Or via User Secrets (preferred for development — keeps secrets out of source control):

```bash
cd AI_Campus_Assistant_System
dotnet user-secrets set "GroqApiKey" "your_groq_api_key_here"
```

**3. Run the app**

```bash
dotnet run
```

The app will seed the database on first startup and be available at **https://localhost:7xxx** / **http://localhost:5xxx** (see console output for the exact port).

---

## 🔧 Configuration

All configuration is in `appsettings.json`:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://mongo:27017",
    "DatabaseName": "AICampusDb"
  },
  "GroqApiKey": "gsk_...",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

| Key | Description |
|---|---|
| `MongoDbSettings:ConnectionString` | MongoDB connection string. Use `mongodb://localhost:27017` for local dev. |
| `MongoDbSettings:DatabaseName` | Name of the MongoDB database (default: `AICampusDb`) |
| `GroqApiKey` | Your Groq API key. Get a free one at [console.groq.com](https://console.groq.com). Without this, chat and AI features will show a configuration message. |

> ⚠️ **Security:** Never commit `appsettings.json` with a real API key to version control. Use `.NET User Secrets` or environment variables in production.

---

## 🔐 Default Credentials

The database is automatically seeded with the following test accounts on first startup:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@campus.edu` | `Admin@123` |
| Teacher | `teacher@campus.edu` | `Teacher@123` |
| Student | `student@campus.edu` | `Student@123` |

> Change these immediately in any non-development environment.

---

## 🗂 Role-Based Modules

### Admin Panel

Accessible at `/Admin/Dashboard` after login.

| Module | Description |
|---|---|
| Dashboard | Live stats — student/teacher counts, courses, fee summaries, complaints, recent users |
| Users | Create, edit, activate/deactivate Admin / Teacher / Student accounts |
| Departments | Add and manage university departments |
| Courses | Create courses, assign teachers, set program/semester |
| Timetable | Schedule classes per program, semester, day, and time slot |
| Fees | Create fee records per student; view payment proofs; verify or reject |
| Fee Requests | Review student fee queries |
| Grades | View all student grades across courses |
| Student Result Card | Print full semester result card for any student |
| Notifications | Broadcast announcements to students or teachers |
| Complaints | View and resolve student complaints |
| FAQs | Manage questions shown in the AI chat knowledge base |
| AI Monitor | View today's AI chat volume and recent queries |

---

### Teacher Portal

Accessible at `/Teacher/Dashboard` after login.

| Module | Description |
|---|---|
| Dashboard | My courses, upcoming lectures, recent submissions |
| Courses | View enrolled/assigned courses |
| Lectures | Upload lecture materials (PDF) per course |
| Assignments | Create assignments with file attachments; set due dates |
| Grade Submissions | Review student submissions and assign marks + feedback |
| Grade Students | Enter Mid, Final, and Assignment marks |
| Attendance | Mark daily attendance per course |
| Attendance Report | View per-student attendance percentage |
| Timetable | View personal teaching schedule |
| Students | Browse students enrolled in your courses |
| Notifications | Send announcements to students |
| Profile | Edit personal info, bio, designation |
| AI Chat | Ask the campus AI assistant questions |
| AI Suggestions | Get AI-generated teaching improvement tips for your courses |
| Content Generator | Use AI to generate quiz/assignment questions for a course |

---

### Student Portal

Accessible at `/Student/Dashboard` after login.

| Module | Description |
|---|---|
| Dashboard | My courses, attendance summary, upcoming deadlines, notifications |
| Lectures | View and download lecture materials for enrolled courses |
| Assignments | View assignments, download question files, submit work |
| Attendance | View personal attendance per course |
| Grades | See Mid, Final, Assignment marks and letter grades |
| Result Card | View or print full semester result card |
| Timetable | Personal class schedule |
| Fees | View fee status; upload payment proofs |
| Fee Challan | Download/print fee challan |
| Notifications | View announcements from Admin/Teachers |
| Complaints | Submit and track complaints |
| Profile | Update personal details, phone, bio |
| AI Chat | Chat with the AI campus assistant (supports Urdu & English) |

---

## 🗄 Database Schema

The following MongoDB collections are used (auto-created on first run):

| Collection | Purpose |
|---|---|
| `Users` | All users (admin, teacher, student). Unique index on `email`. |
| `Courses` | Courses with teacher assignment, program, semester |
| `Departments` | University departments |
| `Semesters` | Semester definitions |
| `Timetables` | Class schedules |
| `Attendances` | Per-student per-course attendance records |
| `Assignments` | Assignment definitions + attached question files |
| `AssignmentSubmissions` | Student submissions with grading fields |
| `Lectures` | Lecture materials (PDF uploads) |
| `Grades` | Mid (30) + Final (50) + Assignment (20) = 100 marks per student/course |
| `fees` | Fee records per student/semester |
| `feeRequests` | Student fee queries |
| `Notifications` | Broadcast messages |
| `ChatQueries` | Logged AI chat interactions |
| `Faqs` | FAQ entries used as AI context |
| `Complaints` | Student complaints + resolution status |

---

## 🤖 AI Integration

The system uses the **Groq API** with the `llama-3.3-70b-versatile` model.

**How it works:**

1. Student/Teacher sends a question via the Chat page.
2. `GroqAiService.AskAsync()` builds a system prompt that identifies the AI as a university campus assistant.
3. Any relevant context (FAQs, course info) can be passed alongside the user's question.
4. The AI always responds in the same language used by the user (Urdu or English).
5. All queries are logged to the `ChatQueries` collection for admin monitoring.

**Teacher AI Suggestions** (`GetTeachingSuggestionsAsync`) auto-generates 3–4 practical tips based on:
- Course name
- Total enrolled students
- Average attendance percentage
- Average grade out of 100

---

## 🔒 Security Notes

- Passwords are hashed using **BCrypt** (work factor 12).
- Authentication uses **HTTP-only, sliding-expiry cookies** (8-hour session).
- All admin/teacher/student routes are protected with `[Authorize(Roles = "...")]`.
- The CSRF token (`ValidateAntiForgeryToken`) is applied to all POST endpoints.
- **Do not** commit `appsettings.json` with real credentials. Use `.NET User Secrets` or environment variables:
  ```bash
  export GroqApiKey="your_key_here"
  export MongoDbSettings__ConnectionString="mongodb://..."
  ```

---

## 🐛 Troubleshooting

**App fails to start — MongoDB connection error**
Ensure MongoDB is running. For Docker Compose, the service name `mongo` is used as the hostname. For local development, change the connection string to `mongodb://localhost:27017`.

**AI Chat returns "AI service not configured"**
Your `GroqApiKey` is missing or empty. Add it to `appsettings.json` or set it as an environment variable. Get a free key at [console.groq.com](https://console.groq.com).

**Seed error on startup (non-fatal)**
A seed warning on startup is safe to ignore — it usually means the data already exists. Check the console for `✅ Database seeded successfully.`

**File uploads not working**
The `wwwroot/uploads/` directory (and sub-folders `assignments/`, `submissions/`, `lectures/`, `payment-proofs/`) must be writable by the app process. In Docker, this is handled automatically.

**Port conflicts**
Default Docker Compose port is `5000`. Change the port mapping in `docker-compose.yml` if it conflicts with another service.

