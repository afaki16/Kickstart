using {{PROJECT_NAME}}.Domain.Common.Enums;
using System;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.DTOs
{
    public class UpdateUserDto
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
