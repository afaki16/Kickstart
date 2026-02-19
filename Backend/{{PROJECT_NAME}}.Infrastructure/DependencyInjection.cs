using {{PROJECT_NAME}}.Application.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Infrastructure.Persistence;
using {{PROJECT_NAME}}.Infrastructure.Identity;
using {{PROJECT_NAME}}.Infrastructure.Repositories;
using {{PROJECT_NAME}}.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace {{PROJECT_NAME}}.Infrastructure
{
    public static class DependencyInjection
    {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddRepositories(typeof(DependencyInjection).Assembly);


        // Add Services
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();



        // Add JWT Authentication
        AddJwtAuthentication(services, configuration);

        return services;
    }

    public static void AddRepositories(this IServiceCollection services, Assembly assembly)
    {
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i.Name.EndsWith("Repository")));

        foreach (var type in repositoryTypes)
        {
            var interfaceType = type.GetInterfaces().First(i => i.Name.EndsWith("Repository"));
            services.TryAddScoped(interfaceType, type);
        }
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Remove delay of token when expire
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Unauthorized",
                            message = "You are not authorized to access this resource"
                        });
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Forbidden",
                            message = "You don't have permission to access this resource"
                        });
                        return context.Response.WriteAsync(result);
                    }
                };
            });

            services.AddAuthorization();
        }
    }
} 
