using FluentValidation;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.CreateTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.UpdateTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.DeleteTenant;

namespace {{PROJECT_NAME}}.Application.Features.Tenants.Commands.CreateTenant;

    public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Domain)
            .MaximumLength(100).WithMessage("Domain must not exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$")
            .When(x => !string.IsNullOrEmpty(x.Domain))
            .WithMessage("Invalid domain format");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Contact email must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage("Contact phone must not exceed 20 characters");
    }
}
