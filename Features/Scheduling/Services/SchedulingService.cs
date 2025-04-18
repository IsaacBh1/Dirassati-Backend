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
    private readonly TimeSpan _lessonDuration = TimeSpan.FromHours(1);

    public AdvancedScheduler(AppDbContext context)
    {
        _context = context;
    }


    public ScheduleResult GenerateSchedule(Guid schoolId, int academicYearId)
    {
        var school = _context.Schools
            .Include(s => s.ScheduleConfig)
            .Include(s => s.Groups)
                .ThenInclude(g => g.Level)
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
            schoolId
        );

        return SimulatedAnnealingOptimizer.Optimize(initialSchedule, 1000, 0.95);
    }


    private void UpdateTimeslots(Data.Models.School school, List<Timeslot> newTimeslots)
    {
        var existingTimeslots = _context.Timeslots
            .Where(t => t.SchoolId == school.SchoolId);
        _context.Timeslots.RemoveRange(existingTimeslots);

        // Add new timeslots
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
            Guid schoolId)
        {
            var result = new ScheduleResult();
            var usedClassroomSlots = new HashSet<(int ClassroomId, int TimeslotId)>();
            var usedTimeslots = new HashSet<int>();
            var usedTeacherSlots = new HashSet<(Guid TeacherId, int TimeslotId)>();
            var hoursTracking = new Dictionary<(int LevelId, int SubjectId), int>();

            foreach (var lsh in levelSubjectHours)
            {
                hoursTracking[(lsh.LevelId, lsh.SubjectId)] = 0;
            }

            var orderedSubjects = levelSubjectHours
                .OrderByDescending(lsh => lsh.Priority)
                .ToList();

            foreach (var group in groups)
            {
                var groupLevelHours = orderedSubjects
                    .Where(lsh => lsh.LevelId == group.Level.LevelId)
                    .ToList();

                foreach (var subjectHours in groupLevelHours)
                {
                    for (int i = 0; i < subjectHours.HoursPerWeek; i++)
                    {
                        var availableSlots = timeslots
                            .Where(ts => ts.SchoolId == schoolId &&
                                  !usedTimeslots.Contains(ts.TimeslotId) &&
                                  IsTeacherAvailable(teachers, ts, subjectHours.SubjectId))
                            .OrderByDescending(ts => ts.IsMorningSlot)
                            .ThenBy(ts => ts.Day)
                            .ThenBy(ts => ts.StartTime)
                            .ToList();

                        foreach (var slot in availableSlots)
                        {
                            var teacher = teachers.FirstOrDefault(t =>
                                t.Subjects.Any(s => s.SubjectId == subjectHours.SubjectId) &&
                                t.Availabilities.Any(a =>
                                    a.Day == slot.Day &&
                                    a.StartTime <= slot.StartTime &&
                                    a.EndTime >= slot.EndTime) &&
                                !usedTeacherSlots.Contains((t.TeacherId, slot.TimeslotId)));

                            var classroom = classrooms.FirstOrDefault(c =>
                                !usedClassroomSlots.Contains((c.ClassroomId, slot.TimeslotId)));

                            if (teacher != null && classroom != null)
                            {
                                var lesson = new Lesson
                                {
                                    SchoolId = schoolId,
                                    GroupId = group.GroupId,
                                    TeacherId = teacher.TeacherId,
                                    ClassroomId = classroom.ClassroomId,
                                    TimeslotId = slot.TimeslotId,
                                    SubjectId = subjectHours.SubjectId,
                                };

                                result.TeacherSchedules.Add(lesson);
                                result.GroupSchedules.Add(lesson);

                                // Update tracking
                                usedTimeslots.Add(slot.TimeslotId);
                                usedClassroomSlots.Add((classroom.ClassroomId, slot.TimeslotId));
                                usedTeacherSlots.Add((teacher.TeacherId, slot.TimeslotId));
                                hoursTracking[(group.Level.LevelId, subjectHours.SubjectId)]++;

                                break; // Lesson created, move to next hour
                            }
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

    private static Classroom FindAvailableClassroom(
        List<Classroom> classrooms,
        Timeslot slot,
        HashSet<(int ClassroomId, int TimeslotId)> usedSlots)
    {
        return classrooms.FirstOrDefault(c =>
            !usedSlots.Contains((c.ClassroomId, slot.TimeslotId)))!;
    }


    private class SimulatedAnnealingOptimizer
    {
        // A local random instance for use in the optimizer
        private static Random _optimizerRandom = new();

        public static ScheduleResult Optimize(ScheduleResult initialSchedule, int iterations, double coolingRate)
        {
            var current = initialSchedule;
            var best = current;
            double temperature = 1.0;

            for (int i = 0; i < iterations; i++)
            {
                var neighbor = GenerateNeighbor(current);
                var currentEnergy = CalculateEnergy(current);
                var neighborEnergy = CalculateEnergy(neighbor);

                // Accept the neighbor if it is better or with a probability depending on the temperature.
                if (neighborEnergy < currentEnergy ||
                    _optimizerRandom.NextDouble() < Math.Exp((currentEnergy - neighborEnergy) / temperature))
                {
                    current = neighbor;
                    if (neighborEnergy < CalculateEnergy(best))
                    {
                        best = neighbor;
                    }
                }

                temperature *= coolingRate;
            }

            best.TotalConflicts = CalculateEnergy(best);
            return best;
        }

        private static ScheduleResult GenerateNeighbor(ScheduleResult schedule)
        {
            // Create a deep clone of the schedule to modify
            var newSchedule = DeepClone(schedule);

            // Make sure we have at least two lessons to swap.
            if (newSchedule.TeacherSchedules.Count < 2)
                return newSchedule;

            int index1 = _optimizerRandom.Next(newSchedule.TeacherSchedules.Count);
            int index2 = _optimizerRandom.Next(newSchedule.TeacherSchedules.Count);

            // Swap two lessons.
            var temp = newSchedule.TeacherSchedules[index1];
            newSchedule.TeacherSchedules[index1] = newSchedule.TeacherSchedules[index2];
            newSchedule.TeacherSchedules[index2] = temp;

            // Reflect the same change in the GroupSchedules if needed.
            // Here we assume that the same lesson instance appears in both teacher and group lists.
            // Adjust accordingly if your design differs.
            return newSchedule;
        }

        private static int CalculateEnergy(ScheduleResult schedule)
        {
            int conflicts = 0;

            // Calculate teacher conflicts: if a teacher is scheduled in more than one lesson at the same timeslot.
            conflicts += schedule.TeacherSchedules
                .GroupBy(l => new { l.TeacherId, l.TimeslotId })
                .Count(g => g.Count() > 1);

            // Calculate classroom conflicts: if a classroom is scheduled more than once at the same timeslot.
            conflicts += schedule.TeacherSchedules
                .GroupBy(l => new { l.ClassroomId, l.TimeslotId })
                .Count(g => g.Count() > 1);

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
                TotalConflicts = source.TotalConflicts
            };
        }
    }
};

