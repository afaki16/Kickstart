using Kickstart.Infrastructure;
using Kickstart.Infrastructure.Persistence;
using Kickstart.Application;
using Kickstart.API.Extensions;
using Kickstart.API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Application services (MediatR, AutoMapper, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.ValidateRequiredSettings(builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();

// Add CORS middleware (must be before authentication)
app.UseCors("DefaultCorsPolicy");

// Authentication and Authorization (order is important)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply pending migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Seed data only if database is empty
await SeedData.SeedAsyncIfEmpty(app.Services);

// Her başlatmada eksik permission'ları ekle
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Kickstart.Infrastructure.Persistence.ApplicationDbContext>();
    await Kickstart.Infrastructure.Persistence.SeedData.SeedPermissionsAsync(context);
}

app.Run();
