using AutoMapper;
using {{PROJECT_NAME}}.Application.Features.Users.Dtos;
using {{PROJECT_NAME}}.Application.Features.Roles.Dtos;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Application.Features.Permissions.Dtos;
using {{PROJECT_NAME}}.Application.Features.Users.Queries.GetAllUsers;
using {{PROJECT_NAME}}.Application.Features.Users.Queries.GetUserById;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Domain.Common.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using {{PROJECT_NAME}}.Domain.Models;

namespace {{PROJECT_NAME}}.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<UserListDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(request.Id);
            
            if (user == null)
            return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "User not found"));

        var userDto = _mapper.Map<UserListDto>(user);
            return Result<UserListDto>.Success(userDto);
        }
    }
} 
