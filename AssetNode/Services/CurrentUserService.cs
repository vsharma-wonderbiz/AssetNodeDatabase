using AssetNode.Interface;
using System.Security.Claims;

namespace AssetNode.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
            ?? "anonymous";

        public string UserName =>
            _httpContextAccessor.HttpContext?.User?.FindFirst("unique_name")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
            ?? "System";

        // Additional helper properties for SignalR
        public string UserDisplayName =>
            _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("given_name")?.Value
            ?? UserName;

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        // Method to get user identifier for SignalR groups
        public string GetUserGroupIdentifier() =>
            $"user_{UserId}";
    }
}