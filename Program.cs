using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Seeders;
using Dirassati_Backend.Extensions;
using Dirassati_Backend.Features.Absences.Services;
using Dirassati_Backend.Features.Auth.SignUp;
using Microsoft.EntityFrameworkCore;
using Persistence;

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

builder.Services.AddSignalR();

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
    await context.Database.MigrateAsync();
    await SchoolSeeder.SeedAsync(registerService);
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
