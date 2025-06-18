using System.Text.Json.Serialization;
using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Middlwares;
using Dirassati_Backend.Configurations;
using Dirassati_Backend.Data.Seeders;
using Dirassati_Backend.Extensions;
using Dirassati_Backend.Features.Abcenses.services;
using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Features.Teachers.Services;
using Dirassati_Backend.Hubs;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Dirassati_Backend.Data;
using Microsoft.AspNetCore.Http.HttpResults;
var builder = WebApplication.CreateBuilder(args);

builder.AddServices();
builder.AddCustomServices();


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Helps in debugging
});

builder.Services.AddRepositories();
builder.Services.AddScoped<AbsenceService>();
builder.Services.Configure<CloudinaryConfig>(builder.Configuration.GetSection("CloudinaryConfig"));
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit number of entries
    options.CompactionPercentage = 0.25; // Remove 25% when limit reached
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // Scan for expired items every 5 minutes
});
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1MB
    options.UseCaseSensitivePaths = false;
    options.SizeLimit = 100 * 1024 * 1024; // 100MB total cache size
});
var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
// Configure Swagger UI only in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dirasati API v1");
    });
    app.MapScalarApiReference(opt => opt
        .WithTitle("Dirasati API Reference")
        .WithOpenApiRoutePattern("/swagger/v1/swagger.json")
        .WithTheme(ScalarTheme.Mars)
        .WithDefaultHttpClient(ScalarTarget.Http, ScalarClient.Http11)

    );
}
//note for front-end : Include the trailing record separator (U+001E) to make the handshake works
using var scope = app.Services.CreateScope();
var service = scope.ServiceProvider;
try
{
    var context = service.GetRequiredService<AppDbContext>();


    await context.Database.MigrateAsync();

    // Check if we already have schools seeded
    if (app.Environment.IsDevelopment())
    {

        var existingSchoolsCount = await context.Schools.CountAsync();
        if (existingSchoolsCount == 0)
        {
            var registerService = service.GetRequiredService<RegisterService>();
            var teacherServices = service.GetRequiredService<TeacherServices>();
            var userManager = service.GetRequiredService<UserManager<AppUser>>();
            await ComprehensiveSeeder.SeedAllSchoolsAsync(registerService, teacherServices, context, userManager);
        }
        else
        {
            Console.WriteLine($"Database already contains {existingSchoolsCount} schools. Skipping seeding.");
        }
    }
}
catch (Exception e)
{
    var logger = service.GetRequiredService<ILogger<Program>>();
    logger.LogError(e, "A problem occurred when migrating database {Error}", e.Message);
    throw new InvalidOperationException("An error occurred during database migration. See inner exception for details.", e);
}

app.MapHub<ParentNotificationHub>("/parentNotificationHub").RequireCors("AllowAll");
app.MapGet("/health", () => Results.Ok("I am healthy"));
app.MapControllers();

await app.RunAsync();
