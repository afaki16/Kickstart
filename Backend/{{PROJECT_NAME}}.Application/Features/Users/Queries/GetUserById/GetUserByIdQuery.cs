using {{PROJECT_NAME}}.Application.Features.Users.Dtos;
using {{PROJECT_NAME}}.Application.Features.Roles.Dtos;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Application.Features.Permissions.Dtos;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System;

namespace {{PROJECT_NAME}}.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<Result<UserListDto>>
    {
        public int Id { get; set; }
    }
} 
