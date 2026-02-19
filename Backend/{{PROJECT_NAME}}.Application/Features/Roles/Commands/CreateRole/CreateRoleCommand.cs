using {{PROJECT_NAME}}.Application.Features.Users.Dtos;
using {{PROJECT_NAME}}.Application.Features.Roles.Dtos;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Application.Features.Permissions.Dtos;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Commands.CreateRole
{
    public class CreateRoleCommand : IRequest<Result<RoleDto>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
} 
