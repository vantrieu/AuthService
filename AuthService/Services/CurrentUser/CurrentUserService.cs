using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

namespace AuthService.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly HttpContext _httpContext;

        private readonly ILogger<CurrentUserService> _logger;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _logger = loggerFactory.CreateLogger<CurrentUserService>();
            Name = GetClaim(httpContextAccessor.HttpContext?.User, "name") ?? string.Empty;
            Email = GetClaim(_httpContext?.User, "email") ?? GetClaim(httpContextAccessor.HttpContext?.User, "upn") ?? string.Empty;
            UserId = GetClaim(httpContextAccessor.HttpContext?.User, "sub") ?? string.Empty;
        }

        public virtual string UserId { get; private set; }

        public virtual string Name { get; private set; }

        public virtual string Email { get; private set; }

        private string GetClaim(ClaimsPrincipal principal, string claimType)
        {
            return principal?.Claims?.FirstOrDefault(p => p.Type == claimType)?.Value;
        }
    }
}
