using {{PROJECT_NAME}}.Application.Features.Roles.Commands.CreateRole;
using {{PROJECT_NAME}}.Application.Features.Roles.Commands.UpdateRole;
using {{PROJECT_NAME}}.Application.Features.Roles.Commands.DeleteRole;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Application.Features.Users.Dtos;
using {{PROJECT_NAME}}.Application.Features.Roles.Dtos;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using {{PROJECT_NAME}}.Application.Features.Permissions.Dtos;
using {{PROJECT_NAME}}.Domain.Common.Enums;
using {{PROJECT_NAME}}.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Roles.Commands.DeleteRole
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
