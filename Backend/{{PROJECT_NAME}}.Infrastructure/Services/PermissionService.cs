using Microsoft.Extensions.Caching.Memory;
using {{PROJECT_NAME}}.Application.Interfaces;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;

namespace {{PROJECT_NAME}}.Infrastructure.Services
{
    public class PermissionService(IMemoryCache cache, IUnitOfWork unitOfWork) : IPermissionService
    {
        private const int CacheExpirationMinutes = 15;

        public async Task<Result<List<string>>> GetUserPermissionsAsync(int userId)
        {
            var cacheKey = $"user_permissions_{userId}";

            if (cache.TryGetValue(cacheKey, out List<string> permissions))
            {
                return Result<List<string>>.Success(permissions);
            }

            var user = await unitOfWork.Users.GetUserWithPermissionsAsync(userId);

            if (user == null)
            {
                return Result<List<string>>.Success(new List<string>());
            }

            permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))
                .Distinct()
                .SelectMany(p =>
                {
                    var perms = new List<string> { p.FullPermission };

                    foreach (var permissionType in p.GetIndividualPermissions())
                    {
                        perms.Add($"{p.Resource}.{permissionType}");
                    }

                    return perms;
                })
                .Distinct()
                .ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));

            cache.Set(cacheKey, permissions, cacheOptions);

            return Result<List<string>>.Success(permissions);
        }

        public async Task<Result<bool>> HasPermissionAsync(int userId, string permission)
        {
            var result = await GetUserPermissionsAsync(userId);

            if (!result.IsSuccess)
                return Result<bool>.Failure(result.Errors.ToArray());

            return Result<bool>.Success(result.Value.Contains(permission, StringComparer.OrdinalIgnoreCase));
        }

        public void ClearUserPermissionCache(int userId)
        {
            var cacheKey = $"user_permissions_{userId}";
            cache.Remove(cacheKey);
        }
    }
}
