using Kickstart.Infrastructure;
using Kickstart.Infrastructure.Persistence;
using Kickstart.Application;
using Kickstart.API.Extensions;
using Kickstart.API.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Suppress "Server: Kestrel" header so the runtime cannot be fingerprinted.
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// Add services to the container.
builder.Services.AddControllers();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure Forwarded Headers (so backend trusts X-Forwarded-* from Nginx/reverse proxy)
// SECURITY: Only trust private/Docker network ranges. Never add public IP ranges here —
// doing so allows IP spoofing from any client that can reach the backend directly.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                             | ForwardedHeaders.XForwardedProto;

    // Clear default networks/proxies; add explicit trusted ranges below.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    // Docker bridge / private LAN ranges where the reverse proxy lives.
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("172.16.0.0"), 12));   // Docker default bridge
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.0.0.0"), 8));      // Common private range
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("192.168.0.0"), 16));  // Common private range

    // Limit forwarded header processing to the immediate proxy hop.
    options.ForwardLimit = 2;
});

// Add Application services (MediatR, AutoMapper, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiServices(builder.Configuration);

// Rate limiting (memory-based, per-IP). Limits are looser in Development
// to keep manual testing painless; Staging/Production stay strict.
builder.Services.AddRateLimiting(builder.Environment);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.ValidateRequiredSettings(builder.Environment);
builder.Configuration.ValidateCorsSettings(builder.Environment);

var app = builder.Build();

// MUST be first: rewrite RemoteIpAddress / scheme from forwarded headers
// before any downstream middleware (logging, auth, rate-limiting) reads them.
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
// Swagger is exposed in Development and Staging only. Production keeps the API surface hidden.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();

// HTTPS redirection only outside Development (local dev runs over HTTP).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

// Security headers run after routing so they apply to every response,
// including auth failures and route-not-found responses.
app.UseSecurityHeaders();

// Add CORS middleware (must be before authentication)
app.UseCors("DefaultCorsPolicy");

// Rate limiting runs before authentication so unauthenticated traffic
// (login, register, forgot-password) is also throttled.
app.UseRateLimiter();

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
