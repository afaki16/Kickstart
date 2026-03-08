using Kickstart.Application.Features.Tenants.Commands.CreateTenant;
using Kickstart.Application.Features.Tenants.Commands.UpdateTenant;
using Kickstart.Application.Features.Tenants.Commands.DeleteTenant;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Domain.Common.Enums;

namespace Kickstart.Application.Features.Tenants.Commands.DeleteTenant;

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
