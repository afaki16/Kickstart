using {{PROJECT_NAME}}.Domain.Common;
using System;

namespace {{PROJECT_NAME}}.Domain.Entities
{
    public class UserRole : BaseEntity
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Role Role { get; set; }
    }
} 
