using {{PROJECT_NAME}}.Application.Features.Users.Dtos;
using {{PROJECT_NAME}}.Application.Features.Roles.Dtos;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Application.Features.Permissions.Dtos;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Permissions.Queries.GetAllPermissions
{
    public class GetAllPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>
    {
    }
} 
