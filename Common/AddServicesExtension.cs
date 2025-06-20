using System.Net.Mail;
using System.Text;
using Dirassati_Backend.Common.Repositories;
using Dirassati_Backend.Common.Security;
using Dirassati_Backend.Configurations;
using Dirassati_Backend.Features.Auth.Accounts.Services;
using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Features.School.Services;
using Dirassati_Backend.Features.SchoolLevels.Services;
using Dirassati_Backend.Features.Students.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Dirassati_Backend.Common.Services.ConnectionTracker;
using Dirassati_Backend.Common.Services.EmailService;
using Dirassati_Backend.Common.Services.PhotoUpload;
using Dirassati_Backend.Features.Payments.Services;
using Dirassati_Backend.Hubs.Services;
using Dirassati_Backend.Data;
using Dirassati_Backend.Features.Classrooms.Services;
using Dirassati_Backend.Features.Groups.Services;
using Dirassati_Backend.Features.Notes.Repos;
using Dirassati_Backend.Features.Notes.Services;
using Dirassati_Backend.Persistence;
using Infrastructure.PhotoUpload;
using Dirassati_Backend.Features.Employees.Services;
using Dirassati_Backend.Common.Services;

namespace Dirassati_Backend.Common;

public static class AddServicesExtension
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerWithJwtAuth();
        builder.Services.AddSingleton<IConnectionTracker, ConnectionTracker>();
        // Register DbContext
        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
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
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        builder.Services.AddHttpContextAccessor();
        var provider = builder.Configuration["Email:Provider"];
        switch (provider)
        {
            case "smtp":
                builder.Services
                    .AddFluentEmail(builder.Configuration["Email:SenderEmail"] ?? throw new InvalidOperationException("Sender Email not found"), builder.Configuration["Email:SenderName"])
                    .AddSmtpSender(new SmtpClient
                    {
                        Host = builder.Configuration["Email:Host"] ?? throw new InvalidOperationException("Check the SMTP server configuration"),
                        Port = builder.Configuration.GetValue<int?>("Email:Port") ?? throw new InvalidOperationException("Check the SMTP server configuration"),
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network
                    });
                break;
            case "postmark":
                {
                    var apiKey = builder.Configuration["Email:PostMartAPI_KEY"];
                    builder.Services
                        .AddFluentEmail(builder.Configuration["Email:SenderEmail"], builder.Configuration["Email:SenderName"])
                        .AddPostmarkSender(apiKey);
                    break;
                }
            default:
                throw new InvalidOperationException("Invalid  Email provider configuration , please choose between smtp or postmark");
        }

        // Add logging
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole(); // Add console logger
            loggingBuilder.AddDebug();   // Add debug logger
        });
        return builder;
    }

    public static WebApplicationBuilder AddCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<RegisterService>();
        builder.Services.AddScoped<SchoolLevelServices>();

        // Use the new EmailTemplateService instead of EmailServices
        builder.Services.AddScoped<IEmailService, EmailTemplateService>();

        builder.Services.AddScoped<ParentServices>();
        builder.Services.AddScoped<StudentServices>();
        builder.Services.AddScoped<VerifyEmailService>();
        builder.Services.AddScoped<SendCridentialsService>();
        builder.Services.AddScoped<ISchoolService, SchoolServices>();
        builder.Services.AddScoped<AccountServices>();
        builder.Services.AddHttpClient<IChargilyClient, ChargilyClient>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<BillServices>();
        builder.Services.AddScoped<IParentNotificationServices, ParentNotificationServices>();
        builder.Services.AddScoped<IGroupServices, GroupServices>();
        builder.Services.AddScoped<IClassroomServices, ClassroomServices>();
        builder.Services.AddScoped<INoteRepository, NoteRepository>();
        builder.Services.AddScoped<INotesServices, NoteService>();
        builder.Services.AddScoped<TokenProvider>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        builder.Services.AddScoped<RefreshTokenProvider>();
        builder.Services.AddScoped<IPhotoUploadService, PhotoUploadService>();
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<CachedServiceBase>();
        return builder;
    }
}
