using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Admin.Dtos;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Constants;
using Kickstart.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Admin.Queries.GetActiveUsersSnapshot
{
    public class GetActiveUsersSnapshotQueryHandler : IRequestHandler<GetActiveUsersSnapshotQuery, Result<List<ActiveUserSnapshotDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetActiveUsersSnapshotQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<List<ActiveUserSnapshotDto>>> Handle(GetActiveUsersSnapshotQuery request, CancellationToken cancellationToken)
        {
            int? tenantId = request.TenantId;
            if (!_currentUserService.CanAccessAllTenants)
            {
                if (tenantId.HasValue && tenantId != _currentUserService.TenantId)
                    return Result<List<ActiveUserSnapshotDto>>.Failure(
                        Error.Failure(ErrorCode.Forbidden, "You can only view active users in your own tenant"));
                tenantId = _currentUserService.TenantId;
            }

            var now = DateTime.UtcNow;

            var usersQuery = _unitOfWork.Users.GetQueryable().Where(x => !x.IsDeleted);
            if (!_currentUserService.CanAccessAllTenants)
                usersQuery = usersQuery.Where(u => !u.UserRoles.Any(ur => ur.Role.Name == RoleNames.SuperAdmin));

            var query =
                from rt in _unitOfWork.RefreshTokens.GetQueryable()
                where !rt.IsRevoked && rt.ExpiryDate > now
                join u in usersQuery on rt.UserId equals u.Id
                join tenant in _unitOfWork.Tenants.GetQueryable() on u.TenantId equals tenant.Id into tj
                from tenant in tj.DefaultIfEmpty()
                select new { rt, u, tenant };

            if (tenantId.HasValue)
                query = query.Where(x => x.u.TenantId == tenantId.Value);

            var rows = await query.ToListAsync(cancellationToken);

            var list = rows
                .GroupBy(x => x.u.Id)
                .Select(g =>
                {
                    var first = g.First();
                    return new ActiveUserSnapshotDto
                    {
                        UserId = g.Key,
                        FullName = first.u.FullName,
                        Email = first.u.Email,
                        TenantId = first.u.TenantId,
                        TenantName = first.tenant?.Name,
                        ActiveSessionCount = g.Count(),
                        LastActivityAt = g.Max(x => x.rt.CreatedDate)
                    };
                })
                .OrderBy(x => x.TenantName ?? "")
                .ThenBy(x => x.FullName)
                .ToList();

            return Result<List<ActiveUserSnapshotDto>>.Success(list);
        }
    }
}
