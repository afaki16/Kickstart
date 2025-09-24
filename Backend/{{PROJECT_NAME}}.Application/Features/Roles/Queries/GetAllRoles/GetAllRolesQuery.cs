using {{PROJECT_NAME}}.Application.DTOs;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Queries
{
    public class GetAllRolesQuery : IRequest<Result<IEnumerable<RoleDto>>>
    {
    }
} 
