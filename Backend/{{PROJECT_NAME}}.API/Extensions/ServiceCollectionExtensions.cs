using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace {{PROJECT_NAME}}.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "{{PROJECT_NAME}} API",
                    Version = "v1",
                    Description = "A base authentication API with JWT, role-based permissions, and user management",
                    Contact = new OpenApiContact
                    {
                        Name = "{{PROJECT_NAME}} Team",
                        Email = "admin@{{PROJECT_NAME}}.com"
                    }
                });

                // Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

            });

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy", policy =>
                {
                    var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? new[] { "*" };
                    
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Add authorization policies
            services.AddAuthorizationPolicies();

            return services;
        }

        private static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Resource-based permission policies
                AddResourcePermissionPolicies(options);
                
                // Role-based policies
                AddRoleBasedPolicies(options);
                
                // Custom policies
                AddCustomPolicies(options);
            });
        }

        private static void AddResourcePermissionPolicies(AuthorizationOptions options)
        {
            // Get all permissions from static constants
            var allPermissions = {{PROJECT_NAME}}.Domain.Constants.Permissions.Helper.GetAllPermissions();
            
            // Generate simplified policy names (e.g., "users.read", "roles.create")
            foreach (var permission in allPermissions)
            {
                var parts = permission.Split('.');
                if (parts.Length == 2)
                {
                    var resource = parts[0].ToLower();
                    var action = parts[1].ToLower();
                    var policyName = $"{resource}.{action}";
                    
                    options.AddPolicy(policyName, policy =>
                        policy.RequireClaim("permission", permission));
                }
            }

            // Add combined permission policies
            AddCombinedPermissionPolicies(options);
        }

        private static void AddCombinedPermissionPolicies(AuthorizationOptions options)
        {
            // Read-Write policies (Read + Create + Update)
            var readWriteResources = new[] { "Users", "Roles", "Permissions" };
            foreach (var resource in readWriteResources)
            {
                var resourceLower = resource.ToLower();
                options.AddPolicy($"{resourceLower}.readwrite", policy =>
                {
                    policy.RequireClaim("permission", $"{resource}.Read");
                    policy.RequireClaim("permission", $"{resource}.Create");
                    policy.RequireClaim("permission", $"{resource}.Update");
                });
            }

            // Full Access policies (Read + Create + Update + Delete)
            var fullAccessResources = new[] { "Users", "Roles", "Permissions" };
            foreach (var resource in fullAccessResources)
            {
                var resourceLower = resource.ToLower();
                options.AddPolicy($"{resourceLower}.fullaccess", policy =>
                {
                    policy.RequireClaim("permission", $"{resource}.Read");
                    policy.RequireClaim("permission", $"{resource}.Create");
                    policy.RequireClaim("permission", $"{resource}.Update");
                    policy.RequireClaim("permission", $"{resource}.Delete");
                });
            }
        }

        private static void AddRoleBasedPolicies(AuthorizationOptions options)
        {
            // Admin role requirement
            options.AddPolicy("RequireAdminRole", policy =>
                policy.RequireRole("Admin", "SuperAdmin"));
            
            options.AddPolicy("RequireSuperAdminRole", policy =>
                policy.RequireRole("SuperAdmin"));
            
            options.AddPolicy("RequireManagerRole", policy =>
                policy.RequireRole("Manager", "Admin", "SuperAdmin"));
            
            options.AddPolicy("RequireUserRole", policy =>
                policy.RequireRole("User", "Manager", "Admin", "SuperAdmin"));
        }

        private static void AddCustomPolicies(AuthorizationOptions options)
        {
            // Custom business logic policies
            options.AddPolicy("RequireActiveUser", policy =>
                policy.RequireAssertion(context =>
                {
                    var user = context.User;
                    var isActive = user.HasClaim(c => c.Type == "status" && c.Value == "Active");
                    return isActive;
                }));

            options.AddPolicy("RequireEmailVerified", policy =>
                policy.RequireAssertion(context =>
                {
                    var user = context.User;
                    var emailVerified = user.HasClaim(c => c.Type == "email_verified" && c.Value == "true");
                    return emailVerified;
                }));

            // Time-based policies
            options.AddPolicy("RequireBusinessHours", policy =>
                policy.RequireAssertion(context =>
                {
                    var currentHour = DateTime.UtcNow.Hour;
                    return currentHour >= 9 && currentHour <= 17; // 9 AM - 5 PM UTC
                }));
        }
    }
} 
