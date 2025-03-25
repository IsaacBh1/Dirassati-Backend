using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Seeders;
using Dirassati_Backend.Extensions;
using Dirassati_Backend.Features.Auth.SignUp;
using Dirassati_Backend.Features.Teachers.Services;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.AddServices();
builder.AddCustomServices();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddRepositories();

var app = builder.Build();

// ✅ Ensure CORS middleware is applied early
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dirasati API v1");
    });
}

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

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
