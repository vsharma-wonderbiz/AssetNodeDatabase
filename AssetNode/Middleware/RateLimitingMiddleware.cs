using System.Collections.Concurrent;

namespace AssetNode.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, List<DateTime>> requestLog = new();
        private const int MaxRequestsPerMinute = 5;
        private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipadd = context.Connection.RemoteIpAddress?.ToString();
            Console.WriteLine($"request from ip address: {ipadd}");

            var currebttime = DateTime.UtcNow;
            Console.WriteLine(currebttime);//prints teh time 

            var requestime = requestLog.GetOrAdd(ipadd, new List<DateTime>());
            Console.WriteLine(requestime.ToArray());
            //creating a list with t eip to store how many time ip called 

            lock (requestime)
            {
                requestime.RemoveAll(t => (currebttime - t) > TimeWindow);

                if (requestime.Count > MaxRequestsPerMinute)
                {
                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                    return;
                }
                requestime.Add(currebttime);
            }

            await _next(context);
        }
    }
}
