using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System;

namespace {{PROJECT_NAME}}.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
} 
