using AssetNode.Interface;
using AssetNode.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AssetNode.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IAssetStorage, AssetStroageService>();
            services.AddScoped<IJsonAssetInterface, JsonServices>();
            services.AddScoped<ISqlInterface, SqlService>();
            services.AddScoped<ISignalInterface, SignalServices>();
            return services;
        }
    }
}
