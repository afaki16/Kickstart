using System;

namespace Kickstart.Application.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        int? TenantId { get; }
        string Email { get; }
        string FullName { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        bool HasPermission(string permission);
    }
} 
