using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Domain.Constants;
using Kickstart.Domain.Entities;
using Kickstart.Infrastructure.Persistence;
using Kickstart.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Repositories
{
    public class RefreshTokenRepository : RepositoryBase<RefreshToken, int>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHash = RefreshTokenHasher.Hash(token);
            return await GetQueryable()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == tokenHash);
        }

        public async Task<RefreshToken> GetActiveTokenByUserIdAsync(int userId)
        {
            return await GetQueryable()
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetUserTokensAsync(int userId, bool includeRevoked = false)
        {
            var query = GetQueryable()
                .Include(rt => rt.User)
                .Where(rt => rt.UserId == userId);

            if (!includeRevoked)
                query = query.Where(rt => !rt.IsRevoked);

            return await query
                .OrderByDescending(rt => rt.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetTokensByDeviceAsync(int userId, string deviceId)
        {
            return await GetQueryable()
                .Include(rt => rt.User)
                .Where(rt => rt.UserId == userId && rt.DeviceId == deviceId)
                .OrderByDescending(rt => rt.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByIpAsync(string ipAddress)
        {
            return await GetQueryable()
                .Include(rt => rt.User)
                .Where(rt => rt.IpAddress == ipAddress && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetActiveTokenCountAsync(int userId)
        {
            return await GetQueryable()
                .CountAsync(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow);
        }

        public async Task<int> GetActiveUserCountAsync(int? tenantId = null, bool excludeUsersWithSuperAdminRole = false)
        {
            // Users with at least one active (non-revoked, non-expired) refresh token
            var activeUserIds = GetQueryable()
                .Where(rt => !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .Select(rt => rt.UserId)
                .Distinct();

            var userQuery = _context.Set<User>()
                .Where(u => activeUserIds.Contains(u.Id));

            if (tenantId.HasValue)
                userQuery = userQuery.Where(u => u.TenantId == tenantId.Value);

            if (excludeUsersWithSuperAdminRole)
                userQuery = userQuery.Where(u => !u.UserRoles.Any(ur => ur.Role.Name == RoleNames.SuperAdmin));

            return await userQuery.CountAsync();
        }

        public async Task<IEnumerable<int>> GetActiveUserIdsAsync(int? tenantId = null, bool excludeUsersWithSuperAdminRole = false)
        {
            var activeUserIds = GetQueryable()
                .Where(rt => !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .Select(rt => rt.UserId)
                .Distinct();

            var userQuery = _context.Set<User>()
                .Where(u => activeUserIds.Contains(u.Id));

            if (tenantId.HasValue)
                userQuery = userQuery.Where(u => u.TenantId == tenantId.Value);

            if (excludeUsersWithSuperAdminRole)
                userQuery = userQuery.Where(u => !u.UserRoles.Any(ur => ur.Role.Name == RoleNames.SuperAdmin));

            return await userQuery.Select(u => u.Id).ToListAsync();
        }

        public async Task RevokeAllUserTokensAsync(int userId, string ipAddress = null, string userAgent = null, string reason = "User logout")
        {
            var tokens = await GetQueryable()
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoke(ipAddress, userAgent, reason);
            }

            UpdateRange(tokens);
        }

        public async Task RevokeTokenAsync(string token, string ipAddress = null, string userAgent = null, string reason = "Token revoked")
        {
            var refreshToken = await GetByTokenAsync(token);
            if (refreshToken != null)
            {
                refreshToken.Revoke(ipAddress, userAgent, reason);
                Update(refreshToken);
            }
        }

        public async Task RevokeTokensByDeviceAsync(int userId, string deviceId, string ipAddress = null, string userAgent = null, string reason = "Device logout")
        {
            var tokens = await GetTokensByDeviceAsync(userId, deviceId);
            var activeTokens = tokens.Where(t => !t.IsRevoked);

            foreach (var token in activeTokens)
            {
                token.Revoke(ipAddress, userAgent, reason);
            }

            UpdateRange(activeTokens);
        }

        public async Task RevokeTokensByIpAsync(string ipAddress, string reason = "Suspicious activity")
        {
            var tokens = await GetActiveTokensByIpAsync(ipAddress);

            foreach (var token in tokens)
            {
                token.Revoke(ipAddress, token.UserAgent, reason);
            }

            UpdateRange(tokens);
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await GetQueryable()
                .Where(rt => rt.ExpiryDate <= DateTime.UtcNow)
                .ToListAsync();

            RemoveRange(expiredTokens);
        }

        public async Task CleanupRevokedTokensAsync(int daysOld = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldRevokedTokens = await GetQueryable()
                .Where(rt => rt.IsRevoked && rt.RevokedDate <= cutoffDate)
                .ToListAsync();

            RemoveRange(oldRevokedTokens);
        }

        public async Task<IEnumerable<RefreshToken>> GetExpiringSoonTokensAsync(int minutesThreshold = 30)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(minutesThreshold);
            return await GetQueryable()
                .Include(rt => rt.User)
                .Where(rt => !rt.IsRevoked && rt.ExpiryDate <= cutoffTime && rt.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();
        }
    }
} 
