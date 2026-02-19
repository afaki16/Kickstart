using {{PROJECT_NAME}}.Application.Features.Auth.Dtos;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Queries.GetUserSessions
{
    public class GetUserSessionsQuery : IRequest<Result<IEnumerable<SessionDto>>>
    {
        public int UserId { get; set; }
    }
} 
