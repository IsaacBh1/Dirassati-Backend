using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Scheduling.Dtos;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Scheduling.Services;

public class ScheduleResult
{
    public List<Lesson> TeacherSchedules { get; set; } = new();
    public List<Lesson> GroupSchedules { get; set; } = new();
    public int TotalConflicts { get; set; }
    public List<SubjectHoursStatus> HoursCompliance { get; set; } = new();
    public List<string> DebugMessages { get; set; } = new(); // Added for debugging
}

public class AdvancedScheduler(AppDbContext context)
{
    private readonly AppDbContext _context = context;
    private readonly TimeSpan _lessonDuration = TimeSpan.FromMinutes(45); // Fixed: 45 minutes, not 60

    public ScheduleResult GenerateSchedule(Guid schoolId, int academicYearId)
    {
        var school = _context.Schools
            .Include(s => s.ScheduleConfig)
            .Include(s => s.Groups)
                .ThenInclude(g => g.Classroom)
                .ThenInclude(c => c!.SchoolLevel)
            .Include(s => s.Teachers)
                .ThenInclude(t => t.Availabilities)
            .Include(s => s.Teachers)
                .ThenInclude(t => t.Subjects)
            .Include(s => s.Classrooms)
            .Include(s => s.Timeslots)
            .FirstOrDefault(s => s.SchoolId == schoolId);

        if (school?.ScheduleConfig == null)
            throw new InvalidOperationException("School configuration missing");

        var timeslots = GenerateTimeSlots(school.ScheduleConfig, schoolId);
        UpdateTimeslots(school, timeslots);

        var levelSubjectHours = _context.LevelSubjectHours
            .Include(lsh => lsh.Subject)
            .Include(lsh => lsh.SchoolLevel)
            .Where(lsh => lsh.SchoolId == schoolId)
            .ToList();

        Console.WriteLine($"Found {levelSubjectHours.Count} level subject hour configurations");
        Console.WriteLine($"Found {school.Groups.Count} groups");
        Console.WriteLine($"Found {school.Teachers.Count} teachers");
        Console.WriteLine($"Generated {timeslots.Count} timeslots");

        var initialSchedule = GreedyScheduler.CreateInitialSchedule(
            [.. school.Groups],
            [.. school.Teachers],
            [.. school.Classrooms],
            timeslots,
            levelSubjectHours,
            schoolId,
            academicYearId
        );

        return SimulatedAnnealingOptimizer.Optimize(initialSchedule, 1000, 0.95);
    }

    private void UpdateTimeslots(Data.Models.School school, List<Timeslot> newTimeslots)
    {
        var existingTimeslots = _context.Timeslots
            .Where(t => t.SchoolId == school.SchoolId);
        _context.Timeslots.RemoveRange(existingTimeslots);

        school.Timeslots = newTimeslots;
        _context.SaveChanges();
    }

    private List<Timeslot> GenerateTimeSlots(SchoolScheduleConfig config, Guid schoolId)
    {
        var slots = new List<Timeslot>();
        var lessonDuration = _lessonDuration;

        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            if (config.DaysOff.Contains(day)) continue;

            TimeSpan start, end;
            bool isSpecial = false;

            if (config.FullDays.Contains(day))
            {
                start = config.MorningStart;
                end = config.AfternoonEnd;
            }
            else if (config.ShortDays.Contains(day))
            {
                start = config.MorningStart;
                end = config.MorningEnd;
                isSpecial = true;
            }
            else continue;

            var current = start;
            while (current + lessonDuration <= end)
            {
                slots.Add(new Timeslot
                {
                    SchoolId = schoolId,
                    Day = day,
                    StartTime = current,
                    EndTime = current + lessonDuration,
                    IsMorningSlot = current < config.MorningEnd,
                    IsSpecialDay = isSpecial
                });

                current = current.Add(lessonDuration);

                // Handle break between morning and afternoon
                if (current == config.MorningEnd && end > config.AfternoonStart)
                {
                    current = config.AfternoonStart;
                }
            }
        }

        Console.WriteLine($"Generated {slots.Count} time slots for school {schoolId}");
        return slots;
    }

    private static class GreedyScheduler
    {
        public static ScheduleResult CreateInitialSchedule(
            List<Group> groups,
            List<Teacher> teachers,
            List<Classroom> classrooms,
            List<Timeslot> timeslots,
            List<LevelSubjectHours> levelSubjectHours,
            Guid schoolId,
            int academicYearId)
        {
            var result = new ScheduleResult();
            var usedClassroomSlots = new HashSet<(Guid ClassroomId, int TimeslotId)>();
            var usedGroupSlots = new HashSet<(Guid GroupId, int TimeslotId)>();
            var usedTeacherSlots = new HashSet<(Guid TeacherId, int TimeslotId)>();
            var hoursTracking = new Dictionary<(int LevelId, int SubjectId), int>();

            // Initialize hours tracking
            foreach (var lsh in levelSubjectHours)
            {
                hoursTracking[(lsh.LevelId, lsh.SubjectId)] = 0;
            }

            var orderedSubjects = levelSubjectHours
                .OrderByDescending(lsh => lsh.Priority)
                .ThenByDescending(lsh => lsh.HoursPerWeek)
                .ToList();

            Console.WriteLine($"Processing {groups.Count} groups");

            foreach (var group in groups)
            {
                Console.WriteLine($"Processing group {group.GroupName} (ID: {group.GroupId})");

                if (group.Classroom?.SchoolLevelId == null)
                {
                    Console.WriteLine($"Group {group.GroupName} has no classroom or school level assigned");
                    result.DebugMessages.Add($"Group {group.GroupName} has no classroom or school level assigned");
                    continue;
                }

                var groupLevelHours = orderedSubjects
                    .Where(lsh => lsh.LevelId == group.Classroom.SchoolLevelId)
                    .ToList();

                Console.WriteLine($"Found {groupLevelHours.Count} subjects for group {group.GroupName} at level {group.Classroom.SchoolLevelId}");

                foreach (var subjectHours in groupLevelHours)
                {
                    Console.WriteLine($"Scheduling {subjectHours.HoursPerWeek} hours of subject {subjectHours.SubjectId} for group {group.GroupName}");

                    for (var i = 0; i < subjectHours.HoursPerWeek; i++)
                    {
                        // Find available teachers for this subject
                        var availableTeachers = teachers
                            .Where(t => t.Subjects != null &&
                                       t.Subjects.Any(s => s.SubjectId == subjectHours.SubjectId) &&
                                       t.Availabilities != null &&
                                       t.Availabilities.Any())
                            .ToList();

                        if (!availableTeachers.Any())
                        {
                            Console.WriteLine($"No teachers available for subject {subjectHours.SubjectId}");
                            result.DebugMessages.Add($"No teachers available for subject {subjectHours.SubjectId}");
                            continue;
                        }

                        Console.WriteLine($"Found {availableTeachers.Count} teachers for subject {subjectHours.SubjectId}");

                        // Find available time slots
                        var availableSlots = timeslots
                            .Where(ts => ts.SchoolId == schoolId &&
                                        !usedGroupSlots.Contains((group.GroupId, ts.TimeslotId)))
                            .OrderByDescending(ts => ts.IsMorningSlot)
                            .ThenBy(ts => ts.Day)
                            .ThenBy(ts => ts.StartTime)
                            .ToList();

                        Console.WriteLine($"Found {availableSlots.Count} available time slots");

                        bool lessonScheduled = false;

                        foreach (var slot in availableSlots)
                        {
                            // Find a teacher available for this slot
                            var teacher = availableTeachers.FirstOrDefault(t =>
                                IsTeacherAvailableForSlot(t, slot) &&
                                !usedTeacherSlots.Contains((t.TeacherId, slot.TimeslotId)));

                            if (teacher == null)
                            {
                                continue; // Try next slot
                            }

                            // Find an available classroom
                            var classroom = classrooms.FirstOrDefault(c =>
                                (c.ClassroomId == group.ClassroomId ||
                                 (c.SchoolLevelId == group.Classroom.SchoolLevelId &&
                                  !usedClassroomSlots.Contains((c.ClassroomId, slot.TimeslotId)))));

                            if (classroom == null)
                            {
                                continue; // Try next slot
                            }

                            // Create the lesson
                            var lesson = new Lesson
                            {
                                SchoolId = schoolId,
                                AcademicYearId = academicYearId,
                                GroupId = group.GroupId,
                                TeacherId = teacher.TeacherId,
                                ClassroomId = classroom.ClassroomId,
                                TimeslotId = slot.TimeslotId,
                                SubjectId = subjectHours.SubjectId,
                            };

                            result.TeacherSchedules.Add(lesson);
                            result.GroupSchedules.Add(lesson);

                            // Mark slots as used
                            usedGroupSlots.Add((group.GroupId, slot.TimeslotId));
                            usedClassroomSlots.Add((classroom.ClassroomId, slot.TimeslotId));
                            usedTeacherSlots.Add((teacher.TeacherId, slot.TimeslotId));

                            // Update hours tracking
                            hoursTracking[(group.Classroom.SchoolLevelId, subjectHours.SubjectId)]++;

                            Console.WriteLine($"Scheduled lesson: Group {group.GroupName}, Subject {subjectHours.SubjectId}, Teacher {teacher.TeacherId}, Slot {slot.Day} {slot.StartTime}");

                            lessonScheduled = true;
                            break;
                        }

                        if (!lessonScheduled)
                        {
                            var debugMsg = $"Could not schedule lesson {i + 1}/{subjectHours.HoursPerWeek} for Group {group.GroupName}, Subject {subjectHours.SubjectId}";
                            Console.WriteLine(debugMsg);
                            result.DebugMessages.Add(debugMsg);

                            // Additional debugging
                            Console.WriteLine($"  - Available teachers: {availableTeachers.Count}");
                            Console.WriteLine($"  - Available slots: {availableSlots.Count}");
                            Console.WriteLine($"  - Used group slots: {usedGroupSlots.Count(x => x.GroupId == group.GroupId)}");
                        }
                    }
                }
            }

            // Create hours compliance report
            result.HoursCompliance = [.. levelSubjectHours.Select(lsh => new SubjectHoursStatus
            {
                LevelId = lsh.LevelId,
                SubjectId = lsh.SubjectId,
                RequiredHours = lsh.HoursPerWeek,
                ScheduledHours = hoursTracking.GetValueOrDefault((lsh.LevelId, lsh.SubjectId), 0)
            })];

            Console.WriteLine($"Final result: {result.TeacherSchedules.Count} lessons scheduled");

            return result;
        }

        private static bool IsTeacherAvailableForSlot(Teacher teacher, Timeslot slot)
        {
            if (teacher.Availabilities == null || !teacher.Availabilities.Any())
            {
                Console.WriteLine($"Teacher {teacher.TeacherId} has no availability set");
                return false;
            }

            var isAvailable = teacher.Availabilities.Any(a =>
                a?.Day == slot.Day &&
                a.StartTime <= slot.StartTime &&
                a.EndTime >= slot.EndTime);

            if (!isAvailable)
            {
                Console.WriteLine($"Teacher {teacher.TeacherId} not available for {slot.Day} {slot.StartTime}-{slot.EndTime}");
            }

            return isAvailable;
        }
    }

    private static bool IsTeacherAvailable(List<Teacher> teachers, Timeslot slot, int subjectId)
    {
        return teachers.Any(t =>
            t.Subjects != null && t.Subjects.Any(s => s.SubjectId == subjectId) &&
            t.Availabilities != null && t.Availabilities.Any(a =>
                a?.Day == slot.Day &&
                a.StartTime <= slot.StartTime &&
                a.EndTime >= slot.EndTime));
    }

    private static class SimulatedAnnealingOptimizer
    {
        private static readonly Random _optimizerRandom = new();

        public static ScheduleResult Optimize(ScheduleResult initialSchedule, int iterations, double coolingRate)
        {
            var current = initialSchedule;
            var best = DeepClone(current);
            double temperature = 1.0;

            for (int i = 0; i < iterations; i++)
            {
                var neighbor = GenerateNeighbor(current);
                var currentEnergy = CalculateEnergy(current);
                var neighborEnergy = CalculateEnergy(neighbor);

                if (neighborEnergy < currentEnergy ||
                    _optimizerRandom.NextDouble() < Math.Exp((currentEnergy - neighborEnergy) / temperature))
                {
                    current = neighbor;
                    if (neighborEnergy < CalculateEnergy(best))
                    {
                        best = DeepClone(neighbor);
                    }
                }

                temperature *= coolingRate;
            }

            best.TotalConflicts = CalculateEnergy(best);
            return best;
        }

        private static ScheduleResult GenerateNeighbor(ScheduleResult schedule)
        {
            var newSchedule = DeepClone(schedule);

            if (newSchedule.TeacherSchedules.Count < 2)
                return newSchedule;

            var attempts = 10;
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                int index1 = _optimizerRandom.Next(newSchedule.TeacherSchedules.Count);
                int index2 = _optimizerRandom.Next(newSchedule.TeacherSchedules.Count);

                if (index1 != index2)
                {
                    var lesson1 = newSchedule.TeacherSchedules[index1];
                    var lesson2 = newSchedule.TeacherSchedules[index2];

                    // Swap timeslots
                    (lesson1.TimeslotId, lesson2.TimeslotId) = (lesson2.TimeslotId, lesson1.TimeslotId);

                    // Update corresponding group schedules
                    var groupLesson1 = newSchedule.GroupSchedules.FirstOrDefault(l =>
                        l.GroupId == lesson1.GroupId && l.SubjectId == lesson1.SubjectId && l.TeacherId == lesson1.TeacherId);
                    var groupLesson2 = newSchedule.GroupSchedules.FirstOrDefault(l =>
                        l.GroupId == lesson2.GroupId && l.SubjectId == lesson2.SubjectId && l.TeacherId == lesson2.TeacherId);

                    if (groupLesson1 != null && groupLesson2 != null)
                    {
                        (groupLesson1.TimeslotId, groupLesson2.TimeslotId) = (groupLesson2.TimeslotId, groupLesson1.TimeslotId);
                    }

                    break;
                }
            }

            return newSchedule;
        }

        private static int CalculateEnergy(ScheduleResult schedule)
        {
            int conflicts = 0;

            // Teacher conflicts (same teacher, same timeslot)
            conflicts += schedule.TeacherSchedules
                .GroupBy(l => new { l.TeacherId, l.TimeslotId })
                .Count(g => g.Count() > 1);

            // Classroom conflicts (same classroom, same timeslot)
            conflicts += schedule.TeacherSchedules
                .GroupBy(l => new { l.ClassroomId, l.TimeslotId })
                .Count(g => g.Count() > 1);

            // Group conflicts (same group, same timeslot)
            conflicts += schedule.GroupSchedules
                .GroupBy(l => new { l.GroupId, l.TimeslotId })
                .Count(g => g.Count() > 1);

            // Hours compliance penalty
            conflicts += schedule.HoursCompliance
                .Where(h => !h.IsFulfilled)
                .Sum(h => Math.Abs(h.RequiredHours - h.ScheduledHours));

            return conflicts;
        }

        private static ScheduleResult DeepClone(ScheduleResult source)
        {
            return new ScheduleResult
            {
                TeacherSchedules = [.. source.TeacherSchedules
                    .Select(l => new Lesson
                    {
                        LessonId = l.LessonId,
                        SchoolId = l.SchoolId,
                        TimeslotId = l.TimeslotId,
                        ClassroomId = l.ClassroomId,
                        GroupId = l.GroupId,
                        SubjectId = l.SubjectId,
                        TeacherId = l.TeacherId,
                        AcademicYearId = l.AcademicYearId
                    })],
                GroupSchedules = [.. source.GroupSchedules
                    .Select(l => new Lesson
                    {
                        LessonId = l.LessonId,
                        SchoolId = l.SchoolId,
                        TimeslotId = l.TimeslotId,
                        ClassroomId = l.ClassroomId,
                        GroupId = l.GroupId,
                        SubjectId = l.SubjectId,
                        TeacherId = l.TeacherId,
                        AcademicYearId = l.AcademicYearId
                    })],
                TotalConflicts = source.TotalConflicts,
                HoursCompliance = [.. source.HoursCompliance
                    .Select(h => new SubjectHoursStatus
                    {
                        LevelId = h.LevelId,
                        SubjectId = h.SubjectId,
                        RequiredHours = h.RequiredHours,
                        ScheduledHours = h.ScheduledHours
                    })],
                DebugMessages = new List<string>(source.DebugMessages)
            };
        }
    }
}