using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands.LogoutAll
{
    public class LogoutAllCommand : IRequest<Result>
    {
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Reason { get; set; }
    }
} 
