using AutoMapper;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.CreateTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.UpdateTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.DeleteTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using {{PROJECT_NAME}}.Domain.Common.Enums;

namespace {{PROJECT_NAME}}.Application.Features.Tenants.Commands.UpdateTenant;

    public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<TenantListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateTenantCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TenantListDto>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetTenantWithUsersAsync(request.Id);

        if (tenant == null)
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.NotFound,
                "Tenant not found"));
        }

        // İsim kontrolü (eğer değiştiyse)
        if (tenant.Name != request.Name && await _unitOfWork.Tenants.NameExistsAsync(request.Name))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                "Tenant name already exists"));
        }

        // Domain kontrolü (eğer değiştiyse)
        if (!string.IsNullOrEmpty(request.Domain) &&
            tenant.Domain != request.Domain &&
            await _unitOfWork.Tenants.DomainExistsAsync(request.Domain))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                "Tenant domain already exists"));
        }

        tenant.Name = request.Name;
        tenant.Description = request.Description;
        tenant.Domain = request.Domain;
        tenant.IsActive = request.IsActive;
        tenant.ContactEmail = request.ContactEmail;
        tenant.ContactPhone = request.ContactPhone;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync();

        var tenantDto = _mapper.Map<TenantListDto>(tenant);
        tenantDto.UserCount = tenant.Users?.Count ?? 0;
        return Result<TenantListDto>.Success(tenantDto);
    }
}
