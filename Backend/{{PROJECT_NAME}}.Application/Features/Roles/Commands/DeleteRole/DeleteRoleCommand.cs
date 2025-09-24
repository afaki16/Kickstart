using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Commands
{
    public class DeleteRoleCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
} 
