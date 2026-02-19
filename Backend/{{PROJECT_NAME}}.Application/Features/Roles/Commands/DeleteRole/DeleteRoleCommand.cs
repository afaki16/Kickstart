using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Commands.DeleteRole
{
    public class DeleteRoleCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
} 
