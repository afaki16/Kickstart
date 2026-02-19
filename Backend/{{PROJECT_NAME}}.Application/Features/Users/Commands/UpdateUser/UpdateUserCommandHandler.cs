using AutoMapper;
using {{PROJECT_NAME}}.Application.Features.Users.Dtos;
using {{PROJECT_NAME}}.Application.Features.Roles.Dtos;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Application.Features.Permissions.Dtos;
using {{PROJECT_NAME}}.Application.Features.Users.Commands.CreateUser;
using {{PROJECT_NAME}}.Application.Features.Users.Commands.UpdateUser;
using {{PROJECT_NAME}}.Application.Features.Users.Commands.DeleteUser;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Domain.Entities;
using {{PROJECT_NAME}}.Domain.Models;
using {{PROJECT_NAME}}.Domain.Common.Enums;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<Application.Features.Users.Dtos.UserListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<UserListDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(request.Id);

            if (user == null)
            return Result<UserListDto>.Failure(Error.Failure(
                             ErrorCode.NotFound,
                             "User not found"));

        // Check if email already exists for another user
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (existingUser != null && existingUser.Id != request.Id)
            return Result<UserListDto>.Failure(Error.Failure(
                        ErrorCode.AlreadyExists,
                        "Email already exists"));

            // Update user properties
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.Status = request.Status;
            user.ProfileImageUrl = request.ProfileImageUrl;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Clear existing roles
            var existingUserRoles = await _unitOfWork.Users.FindAsync(u => u.Id == request.Id);
            var userWithRoles = existingUserRoles.FirstOrDefault();
            if (userWithRoles != null)
            {
                foreach (var userRole in userWithRoles.UserRoles.ToList())
                {
                    _unitOfWork.Users.RemoveUserRole(userRole);
                }
            }

            // Add new roles if provided
            if (request.RoleIds?.Any() == true)
            {
                foreach (var roleId in request.RoleIds)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId
                    };
                    await _unitOfWork.Users.AddUserRoleAsync(userRole);
                }
            }
            await _unitOfWork.SaveChangesAsync();

            // Reload user with roles to get complete data for mapping
            _ = await _unitOfWork.Users.GetUserWithRolesAsync(user.Id);
            var userDto = _mapper.Map<UserListDto>(userWithRoles);
            return Result<UserListDto>.Success(userDto);
        }
    }
} 
