using {{PROJECT_NAME}}.Domain.Common;
using MediatR;
using System;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Commands
{
    public class DeleteRoleCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
} 
