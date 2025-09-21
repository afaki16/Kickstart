using {{PROJECT_NAME}}.Domain.Common;
using MediatR;
using System;

namespace {{PROJECT_NAME}}.Application.Features.Users.Commands
{
    public class DeleteUserCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
} 
