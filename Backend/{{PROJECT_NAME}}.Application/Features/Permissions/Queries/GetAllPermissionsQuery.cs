using {{PROJECT_NAME}}.Application.DTOs;
using {{PROJECT_NAME}}.Domain.Common;
using MediatR;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Permissions.Queries
{
    public class GetAllPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>
    {
    }
} 
