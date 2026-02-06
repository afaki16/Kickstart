using MemberShip.Application.Common.Results;
using MediatR;
using System.Collections.Generic;
using Features.Tenants.Dtos;

namespace Features.Tenants.Queries.GetAllTenants
{
    public class GetAllTenantsQuery : IRequest<Result<IEnumerable<TenantListDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
    }
}
