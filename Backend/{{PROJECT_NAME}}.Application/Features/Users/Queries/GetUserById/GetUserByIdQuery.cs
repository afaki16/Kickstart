using {{PROJECT_NAME}}.Application.DTOs;
using {{PROJECT_NAME}}.Domain.Common;
using MediatR;
using System;

namespace {{PROJECT_NAME}}.Application.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<Result<UserListDto>>
    {
        public int Id { get; set; }
    }
} 
