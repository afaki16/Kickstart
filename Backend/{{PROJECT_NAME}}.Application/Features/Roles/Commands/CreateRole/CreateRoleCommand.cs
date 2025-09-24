using {{PROJECT_NAME}}.Application.DTOs;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Commands
{
    public class CreateRoleCommand : IRequest<Result<RoleDto>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
} 
