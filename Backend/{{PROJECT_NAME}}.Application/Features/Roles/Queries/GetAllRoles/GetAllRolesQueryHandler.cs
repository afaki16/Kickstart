using AutoMapper;
using {{PROJECT_NAME}}.Application.Features.Roles.Queries.GetAllRoles;
using {{PROJECT_NAME}}.Application.Features.Roles.Queries.GetRoleById;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Application.Features.Users.Dtos;
using {{PROJECT_NAME}}.Application.Features.Roles.Dtos;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Application.Features.Permissions.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Queries.GetAllRoles
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<IEnumerable<RoleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllRolesQueryHandler> _logger;

        public GetAllRolesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllRolesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting all roles with permissions...");
                
                // RoleRepository'deki özel metodu kullan
                var roles = await _unitOfWork.Roles.GetAllWithPermissionsAsync();
                
                _logger.LogInformation($"Found {roles.Count()} roles");
                
                var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(roles);
                
                _logger.LogInformation($"Mapped {roleDtos.Count()} role DTOs");

                return Result<IEnumerable<RoleDto>>.Success(roleDtos);
        }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all roles");
                throw;
            }
        }
    }
} 
