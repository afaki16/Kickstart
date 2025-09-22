using {{PROJECT_NAME}}.Application.DTOs;
using {{PROJECT_NAME}}.Domain.Common;
using MediatR;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Queries
{
    public class GetAllRolesQuery : IRequest<Result<IEnumerable<RoleDto>>>
    {
    }
} 
