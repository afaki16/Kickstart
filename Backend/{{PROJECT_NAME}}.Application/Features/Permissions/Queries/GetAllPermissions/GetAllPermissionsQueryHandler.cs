using AutoMapper;
using {{PROJECT_NAME}}.Application.Features.Permissions.Queries;
using {{PROJECT_NAME}}.Application.Interfaces;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Permissions.Handlers
{
    public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Result<IEnumerable<Application.DTOs.PermissionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllPermissionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<Application.DTOs.PermissionDto>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            var permissionDtos = _mapper.Map<IEnumerable<Application.DTOs.PermissionDto>>(permissions);
            return Result.Success(permissionDtos);
        }
    }
} 
