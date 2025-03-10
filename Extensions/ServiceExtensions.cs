using Dirassati_Backend.Features.Parents.Repositories;

namespace Dirassati_Backend.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IParentRepository, ParentRepository>();
            //note for islam : Add other repositories here if needed without create a new file inside Repos folder 
        }
    }
}
