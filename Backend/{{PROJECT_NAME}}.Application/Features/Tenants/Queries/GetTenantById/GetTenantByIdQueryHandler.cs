using AutoMapper;
using MemberShip.Application.Interfaces;
using MemberShip.Application.Common.Results;
using MemberShip.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using MemberShip.Domain.Common.Enums;
using Features.Tenants.Dtos;

namespace Features.Tenants.Queries.GetTenantById
{
    public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTenantByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
        {
            var tenant = await _unitOfWork.Tenants.GetTenantWithUsersAsync(request.Id);
            
            if (tenant == null)
            {
                return Result<TenantDto>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "Tenant not found"));
            }

            var tenantDto = _mapper.Map<TenantDto>(tenant);
            tenantDto.UserCount = tenant.Users?.Count ?? 0;
            return Result<TenantDto>.Success(tenantDto);
        }
    }
}
