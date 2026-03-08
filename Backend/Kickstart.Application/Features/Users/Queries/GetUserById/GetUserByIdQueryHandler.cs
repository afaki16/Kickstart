using AutoMapper;
using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Application.Features.Users.Queries.GetAllUsers;
using Kickstart.Application.Features.Users.Queries.GetUserById;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Common.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Domain.Models;

namespace Kickstart.Application.Features.Users.Queries.GetUserById
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
