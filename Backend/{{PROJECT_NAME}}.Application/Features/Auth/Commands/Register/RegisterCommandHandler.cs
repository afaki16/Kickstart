using {{PROJECT_NAME}}.Application.Interfaces;
using AutoMapper;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.Login;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.Logout;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.LogoutAll;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.LogoutDevice;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.Register;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.RefreshToken;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.RevokeSession;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.ChangePassword;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.ForgotPassword;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.ResetPassword;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Domain.Entities;
using {{PROJECT_NAME}}.Domain.Common.Enums;
using {{PROJECT_NAME}}.Domain.Models;
using {{PROJECT_NAME}}.Application.Features.Users.Dtos;
using {{PROJECT_NAME}}.Application.Features.Roles.Dtos;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Application.Features.Permissions.Dtos;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IMapper _mapper;

        public RegisterCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _mapper = mapper;
        }

        public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if email already exists
            if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
             return Result<UserDto>.Failure(Error.Failure(
                  ErrorCode.AlreadyExists,
                  "Email already exists"));

        // Hash password
        var passwordResult = _passwordService.HashPassword(request.Password);
            if (!passwordResult.IsSuccess)
            return Result<UserDto>.Failure(Error.Failure(
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
                Status = UserStatus.Active,
                EmailConfirmed = false
            };

            // Assign default User role
            var userRole = await _unitOfWork.Roles.GetByNameAsync("User");
            if (userRole != null)
            {
                user.UserRoles.Add(new UserRole { RoleId = userRole.Id });
            }

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
    }
} 
