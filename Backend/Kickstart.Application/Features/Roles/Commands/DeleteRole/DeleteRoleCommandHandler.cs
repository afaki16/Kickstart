using Kickstart.Application.Features.Roles.Commands.CreateRole;
using Kickstart.Application.Features.Roles.Commands.UpdateRole;
using Kickstart.Application.Features.Roles.Commands.DeleteRole;
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

namespace Kickstart.Application.Features.Roles.Commands.DeleteRole
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteRoleCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(request.Id);
            
            if (role == null)
            return Result<RoleDto>.Failure(Error.Failure(
              ErrorCode.NotFound,
              "Role not found"));

        if (role.IsSystemRole)
            return Result<RoleDto>.Failure(Error.Failure(
          ErrorCode.InvalidOperation,
          "Cannot modify system roles"));

        _unitOfWork.Roles.SoftDelete(role);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
} 
