# Dirasati - School Management System (Backend)

## 📌 Project Overview

**Dirasati** is a centralized school management system designed to automate and streamline various administrative, academic, financial, and communication tasks within an educational institution. The platform enables efficient student registration, attendance tracking, timetable management, payment processing, and parent-teacher communication.

## 🚀 Features

### ✅ **Student & Enrollment Management**

- Student profile creation and updates
- Enrollment and re-enrollment management
- Document archiving and academic history tracking

### 📊 **Attendance Tracking**

- Automated attendance
- Real-time absence notifications to parents
- Absence reports and statistics

### 📅 **Timetable Management**

- Automated timetable generation based on teacher and room availability
- Real-time scheduling changes and replacements
- Online timetable access for teachers, students, and parents
- Calendar synchronization with mobile apps

### 💰 **Payment & Accounting**

- Automated tuition fee invoicing based on student level and options
- Payment tracking and overdue alerts
- Secure online payment integrations (credit card, bank transfer, mobile money)
- Scholarship and discount management

### 📢 **Parent & Teacher Communication**

- Web and mobile portals for parents
- Instant notifications, announcements, and event sharing
- Integrated messaging system for teachers and parents

## 🏗️ Architecture & Technologies

### **Backend**

- **Framework**: ASP.NET 9
- **Architecture**: Vertical Slice Architecture with optional CQRS for complex queries
- **Database**: SQL Server / PostgreSQL
- **Security**: JWT Authentication, Role-based access control, Data encryption

## 🚀 Running the Project

### Prerequisites

- Docker and Docker Compose installed (Better use Docker Desktop)
- Git installed
- Access to the repository

### 🐧 Linux/Unix Systems

1. Make the script executable:

```bash
chmod +x Docker/run-backend.sh
```

2. Run the project:

```bash
./Docker/run-backend.sh
```

The script will:

- Pull latest changes from the `main` branch
- Start the Docker containers
- Make the API available at:
  - HTTP: `http://localhost:5080`
  - HTTPS: `https://localhost:5081`

### 🪟 Windows Systems

1. Run the project using the batch script:

```cmd
Docker\run-backend.bat
```

### 🛠️ Manual Docker Commands

If you prefer running Docker commands manually:

1. Build the containers:

```bash
docker-compose build
```

2. Start the application:

```bash
docker-compose up
```

3. Stop the application:

```bash
docker-compose down
```

### 🔄 Development Workflow

1. Access Swagger UI at `http://localhost:5080/swagger`

### 🐞 Troubleshooting

- If ports are already in use, stop other Docker containers or change the port mapping in `docker-compose.yml`
- If you get database errors, ensure the SQLite database file exists
- For connection issues, check if Docker containers are running using `docker ps`
