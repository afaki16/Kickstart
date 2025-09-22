using System;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.DTOs
{
    public class CreateRoleDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
} 
