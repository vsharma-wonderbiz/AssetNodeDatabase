using AssetNode.Middleware;

namespace AssetNode.Extesnions
{
    public static class MiddlewareExtension
    {
        public static IApplicationBuilder UseRateLimitg(this IApplicationBuilder builder)
        {
            //return builder.UseMiddleware<RateLimitingMiddleware>();
            return builder.UseMiddleware<AssetLimiterMiddleware>();
        }
    }
}
