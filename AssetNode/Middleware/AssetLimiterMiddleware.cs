using AssetNode.Interface;

namespace AssetNode.Middleware
{
    public class AssetLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly int AllowedDepth = 10;
        private readonly int AllowedNodes = 200;

        public AssetLimiterMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        
        public async Task InvokeAsync(HttpContext context)
        {
            var scope=_serviceProvider.CreateScope();
            var assetService = scope.ServiceProvider.GetRequiredService<IJsonAssetInterface>();

            int totalCount = assetService.DisplayCount();
            int maxDepth = assetService.MaxDepth();

            if (maxDepth > AllowedDepth || totalCount > AllowedNodes)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Limit exceeded");
                Console.WriteLine("Limit exceeded");
                return;
            }

            await _next(context);
        }
    }
}
