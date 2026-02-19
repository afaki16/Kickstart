using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;

namespace {{PROJECT_NAME}}.Application.Features.Tenants.Commands.DeleteTenant;

    public class DeleteTenantCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
