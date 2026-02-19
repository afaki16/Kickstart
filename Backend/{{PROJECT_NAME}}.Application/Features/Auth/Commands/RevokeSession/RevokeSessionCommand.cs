using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands.RevokeSession
{
    public class RevokeSessionCommand : IRequest<Result>
    {
        public string RefreshToken { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Reason { get; set; }
    }
} 
