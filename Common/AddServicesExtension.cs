using System.Net.Mail;
using System.Text;
using Dirasati_Backend.Configurations;
using Dirassati_Backend.Common.Services;
using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Features.Auth.SignUp;
using Dirassati_Backend.Features.School.Services;
using Dirassati_Backend.Features.SchoolLevels.Services;
using Dirassati_Backend.Features.Students.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence;

namespace Dirassati_Backend.Common;

public static class AddServicesExtension
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddControllers(opt =>
        {
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            opt.Filters.Add(new AuthorizeFilter(policy));
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerWithJwtAuth();

        // Register DbContext
        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found."));
        });

        builder.Services.AddIdentityCore<AppUser>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Register UserManager & SignInManager
        builder.Services.AddScoped<UserManager<AppUser>>();
        builder.Services.AddScoped<SignInManager<AppUser>>();

        // Configure Authentication & JWT
        string key = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key not found");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        builder.Services.AddHttpContextAccessor();
        builder.Services
    .AddFluentEmail(builder.Configuration["Email:SenderEmail"], builder.Configuration["Email:SenderName"])
    .AddSmtpSender(new SmtpClient
    {
        Host = builder.Configuration["Email:Host"] ?? throw new InvalidOperationException("Check the SMTP server configuration"),
        Port = builder.Configuration.GetValue<int?>("Email:Port") ?? throw new InvalidOperationException("Check the SMTP server configuration"),
        EnableSsl = false,
        DeliveryMethod = SmtpDeliveryMethod.Network
    });

        return builder;
    }

    public static WebApplicationBuilder AddCustomServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddScoped<RegisterService>();
        builder.Services.AddScoped<SchoolLevelServices>();
        builder.Services.AddScoped<IEmailService, EmailServices>();
        builder.Services.AddScoped<ParentServices>();
        builder.Services.AddScoped<StudentServices>();
        builder.Services.AddScoped<VerifyEmailService>();
        builder.Services.AddScoped<SendCridentialsService>();
        builder.Services.AddScoped<ISchoolService, SchoolServices>();

        return builder;
    }
}
