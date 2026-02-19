using FluentValidation;
using {{PROJECT_NAME}}.Application.Features.Roles.Commands.CreateRole;
using {{PROJECT_NAME}}.Application.Features.Roles.Commands.UpdateRole;
using {{PROJECT_NAME}}.Application.Features.Roles.Commands.DeleteRole;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Commands.UpdateRole
{
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Role ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
} 
