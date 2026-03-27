using Kickstart.Application.Interfaces;
using AutoMapper;
using Kickstart.Application.Features.Users.Commands.CreateUser;
using Kickstart.Application.Features.Users.Commands.UpdateUser;
using Kickstart.Application.Features.Users.Commands.DeleteUser;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Entities;
using Kickstart.Domain.Models;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Constants;

namespace Kickstart.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CreateUserCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UserListDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // New user is created in current user's tenant. Only SuperAdmin can specify TenantId to create in another tenant.
            int? tenantId;
            if (request.TenantId.HasValue)
            {
                if (!_currentUserService.CanAccessAllTenants)
                    return Result<UserListDto>.Failure(Error.Failure(ErrorCode.Forbidden, "Only SuperAdmin can create users in a specific tenant"));
                tenantId = request.TenantId;
            }
            else
            {
                tenantId = _currentUserService.TenantId;
            }

            // Check if email already exists within this tenant scope
            if (await _unitOfWork.Users.EmailExistsAsync(request.Email, tenantId))
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.AlreadyExists,
                    "Email already exists"));

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
                await _unitOfWork.Users.PhoneExistsAsync(request.PhoneNumber, tenantId))
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.AlreadyExists,
                    "Phone number already exists"));

            if (!_currentUserService.CanAccessAllTenants && request.RoleIds?.Any() == true)
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

            // Hash password
            var passwordResult = _passwordService.HashPassword(request.Password);
            if (!passwordResult.IsSuccess)
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.AlreadyExists,
                    $"{passwordResult.Error}"));

            // Create user
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordResult.Value,
                PhoneNumber = request.PhoneNumber,
                Status = request.Status,
                EmailConfirmed = false,
                TenantId = tenantId
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Assign roles if provided
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
                await _unitOfWork.SaveChangesAsync();
            }

            // Reload user with roles to get complete data for mapping
            var userWithRoles = await _unitOfWork.Users.GetUserWithRolesAsync(user.Id);
            var userDto = _mapper.Map<UserListDto>(userWithRoles);
            return Result<UserListDto>.Success(userDto);
        }
    }
} 
