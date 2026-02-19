using AutoMapper;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.CreateTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.UpdateTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Commands.DeleteTenant;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Domain.Entities;
using {{PROJECT_NAME}}.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using {{PROJECT_NAME}}.Domain.Common.Enums;

namespace {{PROJECT_NAME}}.Application.Features.Tenants.Commands.CreateTenant;

    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTenantCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TenantListDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // İsim kontrolü
        if (await _unitOfWork.Tenants.NameExistsAsync(request.Name))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                "Tenant name already exists"));
        }

        // Domain kontrolü (eğer verilmişse)
        if (!string.IsNullOrEmpty(request.Domain) && await _unitOfWork.Tenants.DomainExistsAsync(request.Domain))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                "Tenant domain already exists"));
        }

        // Entity oluştur
        var tenant = new Tenant
        {
            Name = request.Name,
            Description = request.Description,
            Domain = request.Domain,
            IsActive = request.IsActive,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone
        };

        await _unitOfWork.Tenants.AddAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        var tenantDto = _mapper.Map<TenantListDto>(tenant);
        tenantDto.UserCount = 0; // Yeni tenant'ta henüz user yok
        return Result<TenantListDto>.Success(tenantDto);
    }
}
