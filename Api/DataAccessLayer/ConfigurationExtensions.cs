using Microsoft.Extensions.DependencyInjection;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.DataAccessLayer.Repositories;
using Sidekick.Api.Handlers.IBusiness;

namespace Sidekick.Api.DataAccessLayer
{
    public static class ConfigurationExtensions
    {
        public static void AddDALContexts(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<ICoachRepository, CoachRepository>();
            services.AddScoped<IFacilityUserRepository, FacilityUserRepository>();
            services.AddScoped<IFacilityRepository, FacilityRepository>();
            services.AddScoped<IFacilityPitchRepository, FacilityPitchRepository>();
            services.AddScoped<ISportRepository, SportRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<IFacilityPlayerRepository, FacilityPlayerRepository>();
            services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
            services.AddScoped<IFAQsRepository, FAQsRepository>();
            services.AddScoped<ISurfaceRepository, SurfaceRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<IGymRepository, GymRepository>();
            services.AddScoped<ISizeRepository, SizeRepository>();
            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<IGoalRepository, GoalRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<INationalityRepository, NationalityRepository>();
            services.AddScoped<ILevelRepository, LevelRepository>();
            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<IPlayRepository, PlayRepository>();
            services.AddScoped<IFacilityPitchTimingRepository, FacilityPitchTimingRepository>();
            services.AddScoped<IPromoRepository, PromoRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<ICommissionRepository, CommissionRepository>();
            services.AddScoped<IUserPitchBookingRepository, UserPitchBookingRepository>();
            services.AddScoped<IUserDevicesRepository, UserDevicesRepository>();
            services.AddScoped<ISocialLoginRepository, SocialLoginRepository>();
        }
    }
}
