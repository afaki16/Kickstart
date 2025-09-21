using {{PROJECT_NAME}}.Domain.Common;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Domain.Entities
{
    public class Role : BaseAuditableEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSystemRole { get; set; }

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }

        public Role()
        {
            UserRoles = new HashSet<UserRole>();
            RolePermissions = new HashSet<RolePermission>();
            IsSystemRole = false;
        }
    }
} 
