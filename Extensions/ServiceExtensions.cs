using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Features.Parents.Repositories;
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
            services.AddScoped<TeacherServices>();
            services.AddScoped<SendCridentialsService>(); // Register your credential service too
            //note for islam : Add other repositories here if needed without create a new file inside Repos folder 
        }
    }
}
