using Microsoft.Extensions.DependencyInjection;
using Sidekick.Api.FireBase;
using Sidekick.Api.Helpers.IHelpers;
using System;

namespace Sidekick.Api.Helpers
{
    public static class ServiceExtensions
    {
        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();
            services.AddScoped<IUserHelper, UserHelper>();
            services.AddScoped<ISendEmailHelper, SendEmailHelper>();
            services.AddHttpClient<IFirebaseRepository, FirebaseRepository>().SetHandlerLifetime(TimeSpan.FromMinutes(5));
            services.AddScoped<IPushNotificationTemplateRepository, PushNotificationTemplateRepository>();
        }
    }
}
