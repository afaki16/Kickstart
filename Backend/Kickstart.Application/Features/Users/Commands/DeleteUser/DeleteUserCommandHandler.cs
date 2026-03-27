using Kickstart.Application.Features.Users.Commands.CreateUser;
using Kickstart.Application.Features.Users.Commands.UpdateUser;
using Kickstart.Application.Features.Users.Commands.DeleteUser;
using Kickstart.Application.Common.Authorization;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteUserCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(request.Id);

            if (user == null)
                return Result.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "User not found"));

            // Admin/User can only delete users from their own tenant; SuperAdmin can delete any user
            if (!_currentUserService.CanAccessAllTenants && user.TenantId != _currentUserService.TenantId)
                return Result.Failure(Error.Failure(
                    ErrorCode.Forbidden,
                    "You do not have access to delete this user"));

            if (TenantAdminVisibility.IsHiddenFromTenantAdmin(_currentUserService, user))
                return Result.Failure(Error.Failure(
                    ErrorCode.Forbidden,
                    "You do not have access to delete this user"));

            _unitOfWork.Users.SoftDelete(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
} 
