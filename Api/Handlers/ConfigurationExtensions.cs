using Microsoft.Extensions.DependencyInjection;
using Sidekick.Api.Handlers.Business;
using Sidekick.Api.Handlers.IBusiness;

namespace Sidekick.Api.Handlers
{
    public static class ConfigurationExtensions
    {
        public static void AddHandlerContexts(this IServiceCollection services)
        {
            services.AddScoped<IUserHandler, UserHandler>();
            services.AddScoped<INotificationHandler, NotificationHandler>();
            services.AddScoped<IUserNotificationHandler, UserNotificationHandler>();
            services.AddScoped<IPaymentMethodHandler, PaymentMethodHandler>();
            services.AddScoped<ISendEmailHandler, SendEmailHandler>();
            services.AddScoped<ICoachHandler, CoachHandler>();
            services.AddScoped<IClassHandler, ClassHandler>();
            services.AddScoped<IBookingHandler, BookingHandler>();
            services.AddScoped<IPlayHandler, PlayHandler>();
            services.AddScoped<IPaymentHandler, PaymentHandler>();
            services.AddScoped<IPushNotificationHandler, PushNotificationHandler>();
        }
    }
}
