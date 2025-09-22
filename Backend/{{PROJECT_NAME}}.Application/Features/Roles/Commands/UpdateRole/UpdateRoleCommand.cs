using {{PROJECT_NAME}}.Application.DTOs;
using {{PROJECT_NAME}}.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Commands
{
    public class UpdateRoleCommand : IRequest<Result<RoleDto>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
} 
