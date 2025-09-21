using {{PROJECT_NAME}}.Domain.Enums;
using System;

namespace {{PROJECT_NAME}}.Application.DTOs
{
    public class UpdatePermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Resource { get; set; }
        public PermissionType Type { get; set; }
    }
} 
