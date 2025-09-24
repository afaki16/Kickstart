using {{PROJECT_NAME}}.Application.DTOs;
using {{PROJECT_NAME}}.Application.Common.Results;
using { {PROJECT_NAME}}.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Users.Commands
{
    public class UpdateUserCommand : IRequest<Result<UserListDto>>
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserStatus Status { get; set; }
        public string ProfileImageUrl { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }
} 
