using AutoMapper;
using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Application.Features.Users.Commands.CreateUser;
using Kickstart.Application.Features.Users.Commands.UpdateUser;
using Kickstart.Application.Features.Users.Commands.DeleteUser;
using Kickstart.Application.Common.Authorization;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Entities;
using Kickstart.Domain.Models;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Constants;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<Application.Features.Users.Dtos.UserListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UserListDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(request.Id);

            if (user == null)
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "User not found"));

            // Admin/User can only update users from their own tenant; SuperAdmin can update any user
            if (!_currentUserService.CanAccessAllTenants && user.TenantId != _currentUserService.TenantId)
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.Forbidden,
                    "You do not have access to update this user"));

            if (TenantAdminVisibility.IsHiddenFromTenantAdmin(_currentUserService, user))
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.Forbidden,
                    "You do not have access to update this user"));

            // Check if email already exists for another user in the same tenant
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email, user.TenantId);
            if (existingUser != null && existingUser.Id != request.Id)
            return Result<UserListDto>.Failure(Error.Failure(
                        ErrorCode.AlreadyExists,
                        "Email already exists"));

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
                await _unitOfWork.Users.PhoneExistsAsync(request.PhoneNumber, user.TenantId, request.Id))
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.AlreadyExists,
                    "Phone number already exists"));

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
                if (!_currentUserService.CanAccessAllTenants)
                {
                    foreach (var roleId in request.RoleIds)
                    {
                        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                        if (role != null && role.Name == RoleNames.SuperAdmin)
                            return Result<UserListDto>.Failure(Error.Failure(
                                ErrorCode.Forbidden,
                                "You cannot assign the SuperAdmin role"));
                    }
                }

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
