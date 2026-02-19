using AutoMapper;
using {{PROJECT_NAME}}.Application.Features.Roles.Queries.GetAllRoles;
using {{PROJECT_NAME}}.Application.Features.Roles.Queries.GetRoleById;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using {{PROJECT_NAME}}.Application.Features.Users.Dtos;
using {{PROJECT_NAME}}.Application.Features.Roles.Dtos;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Application.Features.Permissions.Dtos;
using {{PROJECT_NAME}}.Domain.Common.Enums;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Queries.GetRoleById
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(request.Id);
            
            if (role == null)
            return Result<RoleDto>.Failure(Error.Failure(
                       ErrorCode.NotFound,
                       "Role not found"));

        var roleDto = _mapper.Map<RoleDto>(role);
            return Result<RoleDto>.Success(roleDto);
    }
    }
} 
