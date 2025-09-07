﻿using AssetNode.Interface;
using System.Security.Claims;

namespace AssetNode.Services
{
    public class CurrentUserService :ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string UserName =>
    _httpContextAccessor.HttpContext?.User?.FindFirst("unique_name")?.Value
    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
    ?? "System";
    }
}
