using Microsoft.Extensions.DependencyInjection;
using Sidekick.Api.Service.IService;
using Sidekick.Api.Service.Service;

namespace Sidekick.Api.Service
{
    public static class ConfigurationExtensions
    {
        public static void AddServiceContexts(this IServiceCollection services)
        {
            services.AddScoped<ITelRService, TelRService>();
        }
    }
}
