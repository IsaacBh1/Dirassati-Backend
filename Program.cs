using Dirassati_Backend.Common;
using Dirassati_Backend.Configurations;
using Dirassati_Backend.Data.Seeders;
using Dirassati_Backend.Extensions;
using Dirassati_Backend.Features.Absences.Services;
using Dirassati_Backend.Features.Auth.SignUp;
using Dirassati_Backend.Features.Teachers.Services;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServices();
builder.AddCustomServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Helps in debugging
});

builder.Services.AddRepositories();
builder.Services.AddScoped<AbsenceService>();
builder.Services.Configure<CloudinaryConfig>(builder.Configuration.GetSection("CloudinaryConfig"));

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("AllowAll");
//app.UseAuthentication();
app.UseAuthorization();
// Configure Swagger UI only in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dirasati API v1");
    });
}
//note for front-end : Include the trailing record separator (U+001E) to make the handshake works
using var scope = app.Services.CreateScope();
var service = scope.ServiceProvider;
try
{
    var context = service.GetRequiredService<AppDbContext>();
    var registerService = service.GetRequiredService<RegisterService>();
    var teacherServices = service.GetRequiredService<TeacherServices>();
    await context.Database.MigrateAsync();
    string? schoolId = await SchoolSeeder.SeedAsync(registerService, context) ?? throw new Exception("School Has not been created");
    if ((await context.Teachers.FirstOrDefaultAsync(t => t.SchoolId == Guid.Parse(schoolId))) == null)
        await TeacherSeeder.SeedTeachersAsync(Guid.Parse(schoolId), teacherServices);
}
catch (Exception e)
{
    var logger = service.GetRequiredService<ILogger<Program>>();
    logger.LogError(e, "A problem occurred when migrating database");
    throw;
}

app.MapHub<ParentNotificationHub>("/parentNotificationHub").RequireCors("AllowAll");

app.MapControllers();

app.Run();
