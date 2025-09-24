using {{PROJECT_NAME}}.Domain.Common.Enums;
using System;

namespace {{PROJECT_NAME}}.Application.DTOs
{
    public class UserListDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneConfirmed { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
        // Permissions property'si yok - sadece temel bilgiler
    }
}
