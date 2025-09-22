using System;

namespace {{PROJECT_NAME}}.Application.Services
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string Email { get; }
        string FullName { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        bool HasPermission(string permission);
    }
} 
