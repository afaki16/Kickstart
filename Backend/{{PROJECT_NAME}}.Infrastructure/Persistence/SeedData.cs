using {{PROJECT_NAME}}.Domain.Entities;
using {{PROJECT_NAME}}.Domain.Common.Enums;
using {{PROJECT_NAME}}.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace {{PROJECT_NAME}}.Infrastructure.Persistence
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Clear existing data if needed
                await ClearExistingDataAsync(context, logger);

                // Seed Permissions
                await SeedPermissionsAsync(context);

                // Seed Roles
                await SeedRolesAsync(context);

                // Seed Tenants
                await SeedTenantsAsync(context);
                
                // Seed Admin User
                await SeedAdminUserAsync(context);



                logger.LogInformation("Seed data completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding data.");
                throw;
            }
        }

        public static async Task SeedAsyncIfEmpty(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Check if database has any data
                var hasAnyData = await context.Users.AnyAsync() || 
                                await context.Roles.AnyAsync() || 
                                await context.Permissions.AnyAsync() ||
                                await context.Tenants.AnyAsync();
            ;

            if (hasAnyData)
                {
                    logger.LogInformation("Database already contains data. Skipping seed data.");
                    return;
                }

                logger.LogInformation("Database is empty. Starting seed data process...");

                // Seed Permissions
                await SeedPermissionsAsync(context);

                // Seed Roles
                await SeedRolesAsync(context);
                
                // Seed Tenants
                await SeedTenantsAsync(context);
            
                // Seed Admin User
                await SeedAdminUserAsync(context);

                logger.LogInformation("Seed data completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding data.");
                throw;
            }
        }

        private static async Task ClearExistingDataAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Clearing existing data...");

                // Clear in correct order to avoid foreign key constraints
                context.UserRoles.RemoveRange(context.UserRoles);
                context.RolePermissions.RemoveRange(context.RolePermissions);
                context.Users.RemoveRange(context.Users);
                context.Tenants.RemoveRange(context.Tenants);
                context.Roles.RemoveRange(context.Roles);
                context.Permissions.RemoveRange(context.Permissions);
                context.RefreshTokens.RemoveRange(context.RefreshTokens);

                await context.SaveChangesAsync();
                logger.LogInformation("Existing data cleared successfully.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error clearing existing data, continuing with seed...");
            }
        }

        public static async Task SeedPermissionsAsync(ApplicationDbContext context)
        {
            // Get all permissions from static constants
            var allPermissions = Permissions.Helper.GetAllPermissions();

            // Veritabanındaki mevcut permission isimlerini al
            var existingPermissionNames = await context.Permissions
                .Select(p => p.Name)
                .ToListAsync();

            var newPermissions = new List<Permission>();

            foreach (var permissionName in allPermissions)
            {
                if (existingPermissionNames.Contains(permissionName))
                    continue; // Zaten varsa ekleme

                var parts = permissionName.Split('.');
                if (parts.Length == 2)
                {
                    var resource = parts[0];
                    var action = parts[1];
                    var permissionType = MapActionToPermissionType(action);

                    newPermissions.Add(new Permission
                    {
                        Name = permissionName,
                        Description = $"Can {action.ToLower()} {resource.ToLower()}",
                        Resource = resource,
                        Type = permissionType
                    });
                }
            }

            if (newPermissions.Count > 0)
            {
                await context.Permissions.AddRangeAsync(newPermissions);
                await context.SaveChangesAsync();
            }
        }

        private static PermissionType MapActionToPermissionType(string action)
        {
            return action.ToLower() switch
            {
                "read" => PermissionType.Read,
                "create" => PermissionType.Create,
                "update" => PermissionType.Update,
                "delete" => PermissionType.Delete,
                _ => PermissionType.Read // Default to Read
            };
        }

        private static async Task SeedRolesAsync(ApplicationDbContext context)
        {
            var adminRole = new Role
            {
                Name = "Admin",
                Description = "System administrator with full access",
                IsSystemRole = true
            };

            await context.Roles.AddAsync(adminRole);
            await context.SaveChangesAsync();

            // Get all permissions and assign to admin role
            var allPermissions = await context.Permissions.ToListAsync();
            var adminRolePermissions = allPermissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id
            });

            await context.RolePermissions.AddRangeAsync(adminRolePermissions);
            await context.SaveChangesAsync();
        }

    private static async Task SeedTenantsAsync(ApplicationDbContext context)
    {
        // Check if tenants already exist
        if (await context.Tenants.AnyAsync())
        {
            return; // Tenants already seeded
        }

        var defaultTenant = new Tenant
        {
            Name = "Default Tenant",
            Description = "Default system tenant",
            Domain = "default",
            IsActive = true,
            ContactEmail = "admin@default.com",
            ContactPhone = "+905551234567",
            CreatedDate = DateTime.UtcNow
        };

        var demoTenant = new Tenant
        {
            Name = "Demo Tenant",
            Description = "Demo tenant for testing purposes",
            Domain = "demo",
            IsActive = true,
            ContactEmail = "admin@demo.com",
            ContactPhone = "+905559876543",
            CreatedDate = DateTime.UtcNow
        };

        await context.Tenants.AddRangeAsync(defaultTenant, demoTenant);
        await context.SaveChangesAsync();
    }

        private static async Task SeedAdminUserAsync(ApplicationDbContext context)
        {

        // Get default tenant
        var defaultTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Domain == "default");


        var adminUser = new User
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@{{PROJECT_NAME}}.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Default password
                PhoneNumber = "+905551234567",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneConfirmed = true,
            TenantId = defaultTenant?.Id, 
            CreatedDate = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            // Assign admin role to admin user
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                };

                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();
            }
        }
    }
} 
