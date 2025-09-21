using {{PROJECT_NAME}}.Domain.Enums;

namespace {{PROJECT_NAME}}.Application.DTOs
{
    public class CreatePermissionDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Resource { get; set; }
        public PermissionType Type { get; set; }
    }
} 
