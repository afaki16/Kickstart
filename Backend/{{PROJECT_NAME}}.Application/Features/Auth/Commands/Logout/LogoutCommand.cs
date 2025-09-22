using {{PROJECT_NAME}}.Domain.Common;
using MediatR;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands
{
    public class LogoutCommand : IRequest<Result>
    {
        public string RefreshToken { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Reason { get; set; }
    }
} 
