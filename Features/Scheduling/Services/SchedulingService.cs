using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Scheduling.Dtos;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Scheduling.Services;

public class ScheduleResult
{
    // The teacher-centric view: each entry can be interpreted by teacher
    public List<Lesson> TeacherSchedules { get; set; } = new();
    public List<Lesson> GroupSchedules { get; set; } = new();
    public int TotalConflicts { get; set; }
    public List<SubjectHoursStatus> HoursCompliance { get; set; } = new();
}
public class AdvancedScheduler
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();
    private readonly TimeSpan _lessonDuration = TimeSpan.FromMinutes(60); // Fixed: Should be 45 minutes, not 1 hour

/*************  ✨ Windsurf Command ⭐  *************/
/// <summary>
/// Initializes a new instance of the <see cref="AdvancedScheduler"/> class.
/// </summary>
/// <param name="context">The application's database context used for accessing data.</param>

/*******  8954ef3c-acae-4807-bcf0-5877e30180ca  *******/
    public AdvancedScheduler(AppDbContext context)
    {
        _context = context;
    }

    public ScheduleResult GenerateSchedule(Guid schoolId, int academicYearId)
    {
        var school = _context.Schools
            .Include(s => s.ScheduleConfig)
            .Include(s => s.Groups)
                .ThenInclude(g => g.Classroom)
                .ThenInclude(c => c.SchoolLevel)
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
            .Where(lsh => lsh.SchoolId == schoolId)
            .ToList();

        var initialSchedule = GreedyScheduler.CreateInitialSchedule(
            school.Groups.ToList(),
            school.Teachers.ToList(),
            school.Classrooms.ToList(),
            timeslots,
            levelSubjectHours,
            school.ScheduleConfig,
            schoolId,
            academicYearId // Fixed: Pass academicYearId
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
        var lessonDuration = TimeSpan.FromMinutes(45);

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

                // Handle lunch break
                if (current == config.MorningEnd && end > config.AfternoonStart)
                {
                    current = config.AfternoonStart;
                }
            }
        }
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
            SchoolScheduleConfig config,
            Guid schoolId,
            int academicYearId) // Fixed: Added missing parameter
        {
            var result = new ScheduleResult();
            var usedClassroomSlots = new HashSet<(Guid ClassroomId, int TimeslotId)>();
            var usedGroupSlots = new HashSet<(Guid GroupId, int TimeslotId)>(); // Fixed: Track group conflicts
            var usedTeacherSlots = new HashSet<(Guid TeacherId, int TimeslotId)>();
            var hoursTracking = new Dictionary<(int LevelId, int SubjectId), int>();

            // Initialize hours tracking
            foreach (var lsh in levelSubjectHours)
            {
                hoursTracking[(lsh.LevelId, lsh.SubjectId)] = 0;
            }

            var orderedSubjects = levelSubjectHours
                .OrderByDescending(lsh => lsh.Priority)
                .ThenByDescending(lsh => lsh.HoursPerWeek) // Fixed: Consider hours for better scheduling
                .ToList();

            foreach (var group in groups)
            {
                if (group.Classroom?.SchoolLevelId == null) continue; // Fixed: Null check

                var groupLevelHours = orderedSubjects
                    .Where(lsh => lsh.LevelId == group.Classroom.SchoolLevelId)
                    .ToList();

                foreach (var subjectHours in groupLevelHours)
                {
                    for (var i = 0; i < subjectHours.HoursPerWeek; i++)
                    {
                        var availableSlots = timeslots
                            .Where(ts => ts.SchoolId == schoolId &&
                                  !usedGroupSlots.Contains((group.GroupId, ts.TimeslotId)) && // Fixed: Check group conflicts
                                  IsTeacherAvailable(teachers, ts, subjectHours.SubjectId) &&
                                  !usedClassroomSlots.Contains((group.ClassroomId, ts.TimeslotId))) // Fixed: Check classroom availability
                            .OrderByDescending(ts => ts.IsMorningSlot)
                            .ThenBy(ts => ts.Day)
                            .ThenBy(ts => ts.StartTime)
                            .ToList();

                        bool lessonScheduled = false;
                        foreach (var slot in availableSlots)
                        {
                            var teacher = teachers.FirstOrDefault(t =>
                                t.Subjects.Any(s => s.SubjectId == subjectHours.SubjectId) &&
                                IsTeacherAvailableForSlot(t, slot) &&
                                !usedTeacherSlots.Contains((t.TeacherId, slot.TimeslotId)));

                            var classroom = classrooms.FirstOrDefault(c =>
                                c.ClassroomId == group.ClassroomId ||
                                (!usedClassroomSlots.Contains((c.ClassroomId, slot.TimeslotId)) &&
                                 c.SchoolLevelId == group.Classroom.SchoolLevelId)); // Fixed: Better classroom selection

                            if (teacher != null && classroom != null)
                            {
                                var lesson = new Lesson
                                {
                                    SchoolId = schoolId,
                                    AcademicYearId = academicYearId, // Fixed: Set academic year
                                    GroupId = group.GroupId,
                                    TeacherId = teacher.TeacherId,
                                    ClassroomId = classroom.ClassroomId,
                                    TimeslotId = slot.TimeslotId,
                                    SubjectId = subjectHours.SubjectId,
                                };

                                result.TeacherSchedules.Add(lesson);
                                result.GroupSchedules.Add(lesson);

                                // Update tracking
                                usedGroupSlots.Add((group.GroupId, slot.TimeslotId)); // Fixed: Track group usage
                                usedClassroomSlots.Add((classroom.ClassroomId, slot.TimeslotId));
                                usedTeacherSlots.Add((teacher.TeacherId, slot.TimeslotId));
                                hoursTracking[(group.Classroom.SchoolLevelId, subjectHours.SubjectId)]++;

                                lessonScheduled = true;
                                break; // Lesson created, move to next hour
                            }
                        }

                        if (!lessonScheduled)
                        {
                            // Log or handle the case where a lesson couldn't be scheduled
                            System.Diagnostics.Debug.WriteLine(
                                $"Could not schedule lesson for Group {group.GroupName}, Subject {subjectHours.SubjectId}, Hour {i + 1}");
                        }
                    }
                }
            }

            result.HoursCompliance = levelSubjectHours.Select(lsh => new SubjectHoursStatus
            {
                LevelId = lsh.LevelId,
                SubjectId = lsh.SubjectId,
                RequiredHours = lsh.HoursPerWeek,
                ScheduledHours = hoursTracking.GetValueOrDefault((lsh.LevelId, lsh.SubjectId), 0)
            }).ToList();

            return result;
        }

        private static bool IsTeacherAvailableForSlot(Teacher teacher, Timeslot slot)
        {
            return teacher.Availabilities.Any(a =>
                a.Day == slot.Day &&
                a.StartTime <= slot.StartTime &&
                a.EndTime >= slot.EndTime);
        }
    }

    private static bool IsTeacherAvailable(List<Teacher> teachers, Timeslot slot, int subjectId)
    {
        return teachers.Any(t =>
            t.Subjects.Any(s => s.SubjectId == subjectId) &&
            t.Availabilities.Any(a =>
                a.Day == slot.Day &&
                a.StartTime <= slot.StartTime &&
                a.EndTime >= slot.EndTime));
    }

    private static class SimulatedAnnealingOptimizer
    {
        private static readonly Random _optimizerRandom = new();

        public static ScheduleResult Optimize(ScheduleResult initialSchedule, int iterations, double coolingRate)
        {
            var current = initialSchedule;
            var best = DeepClone(current); // Fixed: Clone the best solution
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
                        best = DeepClone(neighbor); // Fixed: Clone the new best solution
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

            // Fixed: More intelligent neighbor generation
            var attempts = 10;
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                int index1 = _optimizerRandom.Next(newSchedule.TeacherSchedules.Count);
                int index2 = _optimizerRandom.Next(newSchedule.TeacherSchedules.Count);

                if (index1 != index2)
                {
                    // Try swapping timeslots instead of entire lessons
                    var lesson1 = newSchedule.TeacherSchedules[index1];
                    var lesson2 = newSchedule.TeacherSchedules[index2];

                    // Swap timeslots
                    (lesson1.TimeslotId, lesson2.TimeslotId) = (lesson2.TimeslotId, lesson1.TimeslotId);

                    // Update the corresponding lessons in GroupSchedules
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

            // Teacher conflicts
            conflicts += schedule.TeacherSchedules
                .GroupBy(l => new { l.TeacherId, l.TimeslotId })
                .Count(g => g.Count() > 1);

            // Classroom conflicts
            conflicts += schedule.TeacherSchedules
                .GroupBy(l => new { l.ClassroomId, l.TimeslotId })
                .Count(g => g.Count() > 1);

            // Group conflicts (students can't be in two places at once)
            conflicts += schedule.GroupSchedules
                .GroupBy(l => new { l.GroupId, l.TimeslotId })
                .Count(g => g.Count() > 1);

            // Fixed: Add penalty for unfulfilled hours
            conflicts += schedule.HoursCompliance
                .Where(h => !h.IsFulfilled)
                .Sum(h => h.RequiredHours - h.ScheduledHours);

            return conflicts;
        }

        private static ScheduleResult DeepClone(ScheduleResult source)
        {
            return new ScheduleResult
            {
                TeacherSchedules = source.TeacherSchedules
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
                    }).ToList(),
                GroupSchedules = source.GroupSchedules
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
                    }).ToList(),
                TotalConflicts = source.TotalConflicts,
                HoursCompliance = source.HoursCompliance
                    .Select(h => new SubjectHoursStatus
                    {
                        LevelId = h.LevelId,
                        SubjectId = h.SubjectId,
                        RequiredHours = h.RequiredHours,
                        ScheduledHours = h.ScheduledHours
                    }).ToList()
            };
        }
    }
}