using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.CreateTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.UpdateTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.DeleteTenant;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using {{PROJECT_NAME}}.Domain.Common.Enums;

namespace {{PROJECT_NAME}}.Application.Features.Tenants.Commands.DeleteTenant;

    public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTenantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(request.Id);

        if (tenant == null)
        {
            return Result<bool>.Failure(Error.Failure(
                ErrorCode.NotFound,
                "Tenant not found"));
        }

        // Soft delete - User'ların TenantId'si null olur (DeleteBehavior.SetNull)
        _unitOfWork.Tenants.SoftDelete(tenant);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
