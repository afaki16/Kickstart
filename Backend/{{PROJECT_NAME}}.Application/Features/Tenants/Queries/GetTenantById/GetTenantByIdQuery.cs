using MemberShip.Application.Common.Results;
using MediatR;
using Features.Tenants.Dtos;

namespace Features.Tenants.Queries.GetTenantById
{
    public class GetTenantByIdQuery : IRequest<Result<TenantDto>>
    {
        public int Id { get; set; }
    }
}
