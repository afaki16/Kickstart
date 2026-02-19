using {{PROJECT_NAME}}.Infrastructure;
using {{PROJECT_NAME}}.Infrastructure.Persistence;
using {{PROJECT_NAME}}.Application;
using {{PROJECT_NAME}}.API.Extensions;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
}

app.UseHttpsRedirection();

// Add CORS middleware (must be before authentication)
app.UseCors("DefaultCorsPolicy");

// Authentication and Authorization (order is important)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed data only if database is empty
await SeedData.SeedAsyncIfEmpty(app.Services);

// Her başlatmada eksik permission'ları ekle
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<{{PROJECT_NAME}}.Infrastructure.Persistence.ApplicationDbContext>();
    await {{PROJECT_NAME}}.Infrastructure.Persistence.SeedData.SeedPermissionsAsync(context);
}

app.Run();
