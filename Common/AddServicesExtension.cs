using System;
using Dirasati_Backend.Configurations;
using Dirassati_Backend.Features.Auth.SignUp;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Dirassati_Backend.Common;

public static class AddServicesExtension
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerWithJwtAuth();
        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found."));
        });
        builder.Services.AddIdentityCore<AppUser>()
        .AddEntityFrameworkStores<AppDbContext>();
        builder.Services.AddAuthentication();
        builder.Services.AddScoped<RegisterService>();
        return builder;
    }

}
