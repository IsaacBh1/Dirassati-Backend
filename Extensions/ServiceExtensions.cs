using Dirassati_Backend.Domain.Services;
using Dirassati_Backend.Features.Absences.Repos;
using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Features.Groups.Repos;
using Dirassati_Backend.Features.Notes.Repos;
using Dirassati_Backend.Features.Parents.Repositories;
using Dirassati_Backend.Features.Scheduling.Services;
using Dirassati_Backend.Features.Students.Repositories;
using Dirassati_Backend.Features.Teachers.Services;
using Dirassati_Backend.Persistence.Services;
using Dirassati_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IParentRepository, ParentRepository>();
            services.AddScoped<ITeacherRepository, TeacherRepository>();
            services.AddScoped<IParentRepository, ParentRepository>();
            services.AddScoped<IAbsenceRepository, AbsenceRepository>();
            services.AddScoped<TeacherServices>();
            services.AddScoped<AdvancedScheduler>();
            services.AddScoped<SendCridentialsService>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<ICsvService , CsvService>();
            services.AddHttpContextAccessor();

        }
    }
}
