using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Application.Features.Tenants.Dtos;
using MediatR;
namespace {{PROJECT_NAME}}.Application.Features.Tenants.Queries.GetTenantById
{
    public class GetTenantByIdQuery : IRequest<Result<TenantDto>>
    {
        public int Id { get; set; }
    }
}
