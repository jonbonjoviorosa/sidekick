using AutoMapper;
using Sidekick.Model;
using Sidekick.Model.Booking;
using Sidekick.Model.Class;
using Sidekick.Model.Notification;
using Sidekick.Model.PaymentMethod;
using Sidekick.Model.UserNotification;
using System;

namespace Sidekick.Api.Configurations.Mapper
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<NotificationViewModel, Notification>();
            CreateMap<Notification, NotificationViewModel>();
            CreateMap<UserNotification, UserNotificationViewModel>();
            CreateMap<UserNotificationViewModel, UserNotification>();
            CreateMap<User, UserProfile>();
            CreateMap<UserPlayBadge, UserPlayBadgeViewModel>();
            CreateMap<UserTrainBadge, UserTrainBadgeViewModel>();
            CreateMap<PaymentMethod_Card, PaymentMethod_CardViewModel>();
            CreateMap<PaymentMethod_CardViewModel, PaymentMethod_Card>();
            CreateMap<CoachCustomSchedule, CoachCustomScheduleViewModel>();
            CreateMap<CoachEverydaySchedule, CoachEverydayScheduleViewModel>();
            CreateMap<CoachUpdateProfile, CoachScheduleViewModel>()
                .ForMember(dest => dest.EverydaySchedule, opt => opt.MapFrom(x => x.EverydaySched))
                .ForMember(dest => dest.CustomSchedule, opt => opt.MapFrom(x => x.CustomSched));
            CreateMap<IndividualClass, IndividualClassViewModel>();
            CreateMap<IndividualClassViewModel, IndividualClass>();
            CreateMap<GroupClass, GroupClassViewModel>();
            CreateMap<GroupClassViewModel, GroupClass>();
            CreateMap<GroupBooking_UpdateStatusViewModel, GroupBooking>();
            CreateMap<IndividualBooking_SaveViewModel, IndividualBooking>();
            CreateMap<Coach, CoachProfile>();
            CreateMap<Guid, UserGoal>()
                .ForMember(dest => dest.GoalId, opt => opt.MapFrom(x => x));
            CreateMap<IndividualClassDetailsViewModel, IndividualClassDetails>();
            CreateMap<IndividualClassDetails, IndividualClassDetailsViewModel>();
            CreateMap<FacilityPitch, FacilityPitchVM>();
            CreateMap<IndividualBooking, IndividualConfirmBookingViewModel>()
                .ForMember(dest => dest.IndivdualClassId, opt => opt.MapFrom(x => x.ClassId))
                .ForMember(dest => dest.PricePerHour, opt => opt.MapFrom(x => x.AmountPerHour))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(x => x.TotalAmount));
        }
    }
}
