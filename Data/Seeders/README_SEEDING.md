# Comprehensive Database Seeder Documentation

## Overview

I've created a comprehensive database seeding system that populates your Dirasati database with realistic test data for multiple schools across Algeria. This system creates a complete educational ecosystem with schools, administrators, teachers, students, parents, classrooms, and groups.

## What Gets Seeded

### Schools (10 Total)

- **4 Primary Schools (Primaire)**: El Nour, Al Amal, Tarek Ibn Ziad, Ibn Sina
- **3 Middle Schools (Moyenne)**: Mohamed Boudiaf, Emir Abdelkader, Ahmed Zabana
- **3 High Schools (Lycee)**: Houari Boumediene, Frantz Fanon, Abane Ramdane

### School Details

- **Locations**: Distributed across 18 Algerian states (Algiers, Oran, Constantine, Batna, etc.)
- **Algerian Names**: All personnel use authentic Algerian names written in English
- **Phone Numbers**: Proper Algerian phone format (+213...)
- **Addresses**: Realistic Algerian addresses with proper postal codes
- **Academic Years**: Varied start/end dates for different schools
- **Billing**: School-type appropriate fee structures

### Personnel Per School

- **1 Administrator**: School director with full permissions
- **8-12 Teachers**: Mixed gender with various contract types and hire dates
- **15-20 Families**: Each with 1-3 children (20+ students per school)

### Educational Structure

- **Age-Appropriate Students**:
  - Primary: Ages 6-11
  - Middle: Ages 11-15
  - High School: Ages 15-18
- **Proper Level Assignment**: Students assigned to correct school levels based on age
- **Specializations**: High school students get specializations (Science, Letters, etc.)
- **Classrooms & Groups**: Each classroom has one group, organized by level and specialization

### Parent-Student Relationships

- **Realistic Families**: Parents with 1-3 children
- **Sibling Relationships**: Some students share the same parents
- **Complete Parent Profiles**: Including occupation, national ID, contact info
- **Parent Accounts**: Login-ready parent accounts with default password "Parent@123"

## Files Created/Modified

### New Seeders

1. **`ComprehensiveSeeder.cs`** - Main orchestrator that coordinates all seeding
2. **`ParentSeeder.cs`** - Creates parent accounts and student records
3. **`ClassroomSeeder.cs`** - Creates classrooms and groups, assigns students

### Enhanced Seeders

4. **`TeacherSeeder.cs`** - Enhanced with Algerian names and realistic data

### Updated Files

5. **`Program.cs`** - Modified to use the comprehensive seeding system

## How to Use

### Automatic Seeding

The seeding runs automatically when you start the application. If the database is empty, it will:

1. Run database migrations
2. Seed all 10 schools with complete data
3. Create all associated records (teachers, students, parents, classrooms, groups)

### Manual Seeding

To force re-seeding, clear your database and restart the application.

## Login Credentials

### School Administrators

- **Email**: `{firstname}.{lastname}@{schoolname}.dz`
- **Password**: `Admin@123`
- **Example**: `ahmed.benali@elnourprimaryschool.dz`

### Teachers

- **Email**: `{firstname}.{lastname}@teacher.dz`
- **Password**: Auto-generated during teacher registration
- **Example**: `fatima.cherif@teacher.dz`

### Parents

- **Email**: `{firstname}.{lastname}@email.dz`
- **Password**: `Parent@123`
- **Example**: `mohamed.djebbar@email.dz`

## Database Schema Populated

### Core Entities

- Schools (10)
- School Types (3)
- School Levels (12)
- Specializations (10)
- Academic Years (10)
- Addresses (10+ for schools, many for users)

### Personnel

- Employees (10 administrators)
- Teachers (80-120 total)
- Parents (150-200 total)

### Students & Organization

- Students (200+ total)
- Groups (30-40 total)
- Classrooms (30-40 total)
- Student-Group assignments

### Supporting Data

- Phone Numbers
- Contract Types
- Subjects (auto-seeded via existing seeders)
- Parent Relationship Types

## Testing Scenarios Enabled

With this comprehensive data, you can test:

1. **Multi-school Management**: Different school types with varied configurations
2. **User Role Testing**: Administrators, teachers, and parents across multiple schools
3. **Student Management**: Age-appropriate level assignments and specializations
4. **Family Relationships**: Parents with multiple children, sibling management
5. **Classroom Organization**: Group capacity, level-based organization
6. **Geographic Distribution**: Schools across different Algerian states
7. **Billing Scenarios**: Different fee structures by school type
8. **Academic Calendar**: Varied academic year dates
9. **Contact Management**: Phone numbers, addresses, emergency contacts
10. **Authentication**: Login testing for all user types

## Notes

- All data uses realistic Algerian names, places, and formats
- Phone numbers follow Algerian numbering conventions
- National ID numbers are properly formatted (11 digits)
- Students are automatically assigned to appropriate groups based on level and specialization
- The system prevents duplicate seeding - it only runs if the database is empty
- All relationships are properly maintained (foreign keys, navigation properties)

This seeding system provides a rich, realistic dataset that covers all major use cases for testing your educational management application.
