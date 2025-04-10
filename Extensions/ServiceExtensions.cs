using Dirassati_Backend.Features.Absences.Repos;
using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Features.Groups.Repos;
using Dirassati_Backend.Features.Parents.Repositories;
using Dirassati_Backend.Features.Students.Repositories;
using Dirassati_Backend.Features.Teachers.Services;
using Dirassati_Backend.Repositories;

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
            services.AddScoped<SendCridentialsService>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();

        }
    }
}
