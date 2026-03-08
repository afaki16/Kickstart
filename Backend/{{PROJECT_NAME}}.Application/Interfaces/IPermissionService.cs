using {{PROJECT_NAME}}.Application.Common.Results;

namespace {{PROJECT_NAME}}.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<Result<bool>> HasPermissionAsync(int userId, string permission);
        Task<Result<List<string>>> GetUserPermissionsAsync(int userId);
        void ClearUserPermissionCache(int userId);
    }
}
