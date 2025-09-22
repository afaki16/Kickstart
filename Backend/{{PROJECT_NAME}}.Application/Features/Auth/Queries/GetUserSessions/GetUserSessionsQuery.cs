using {{PROJECT_NAME}}.Application.DTOs.Auth;
using {{PROJECT_NAME}}.Domain.Common;
using MediatR;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Queries
{
    public class GetUserSessionsQuery : IRequest<Result<IEnumerable<SessionDto>>>
    {
        public int UserId { get; set; }
    }
} 
