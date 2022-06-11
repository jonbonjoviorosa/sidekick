using Sidekick.Api.Helpers;
using Microsoft.EntityFrameworkCore;
using Sidekick.Model;
using Sidekick.Model.Notification;
using Sidekick.Model.PaymentMethod;
using Sidekick.Model.Payment;
using Sidekick.Model.Specialty;
using Sidekick.Model.Gym;
using Sidekick.Model.Language;
using Sidekick.Model.SetupConfiguration.Size;
using Sidekick.Model.Class;
using Sidekick.Model.Booking;
using Sidekick.Model.Nationality;
using Sidekick.Model.SetupConfiguration.Goals;
using Sidekick.Model.SetupConfiguration.Level;
using Sidekick.Model.Promo;
using Sidekick.Model.Chat;
using Sidekick.Model.UserNotification;

namespace Sidekick.Api.DataAccessLayer
{
    public class APIDBContext : DbContext
    {
        public APIConfigurationManager MasterConf { get; set; }

        public APIDBContext(DbContextOptions<APIDBContext> options, APIConfigurationManager _acm) : base(options) { MasterConf = _acm; }

        // admin
        public DbSet<Admin> Admins { get; set; }
        public DbSet<AdminLoginTransaction> AdminLoginTransactions { get; set; }

        // facility
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<FacilityUser> FacilityUsers { get; set; }
        public DbSet<FacilityUserLoginTransaction> FacilityUserLoginTransactions { get; set; }
        public DbSet<FacilityTiming> FacilityTimings { get; set; }
        public DbSet<FacilitySport> FacilitySports { get; set; }
        public DbSet<FacilityPitch> FacilityPitches { get; set; }
        public DbSet<UnavailableSlot> UnavailableSlots { get; set; }
        public DbSet<FacilityPitchTiming> FacilityPitchTimings { get; set; }
        public DbSet<FacilityPlayer> FacilityPlayers { get; set; }
        public DbSet<FacilityUserType> FacilityUserTypes { get; set; }

        // pitch
        public DbSet<FieldPitch> FieldPitches { get; set; }
        public DbSet<FieldPitchBooking> FieldPitchBookings { get; set; }
        public DbSet<UserPitchBooking> UserPitchBookings { get; set; }
        public DbSet<FieldPitchParticipant> FieldPitchParticipants { get; set; }
        public DbSet<UserPitchParticipant> UserPitchParticipants { get; set; }

        // training
        public DbSet<Training> Trainings { get; set; }
        public DbSet<TrainingTiming> TrainingTimings { get; set; }
        public DbSet<TrainingBooking> TrainingBookings { get; set; }

        // user
        public DbSet<User> Users { get; set; }
        public DbSet<UserPlayBadge> UserPlayBadges { get; set; }
        public DbSet<UserTrainBadge> UserTrainBadges { get; set; }
        public DbSet<UserGoal> UserGoals { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
        public DbSet<UserFriendRequest> UserFriendRequests { get; set; }
        public DbSet<UserTeam> UserTeams { get; set; }
        public DbSet<UserTeamMember> UserTeamMembers { get; set; }
        public DbSet<BlockedUser> BlockedUsers { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<UserLoginTransaction> UserLoginTransactions { get; set; }
        public DbSet<UserVerificationCode> UserVerificationCodes { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<Report> Reports { get; set; }

        public DbSet<Request> Requests { get; set; }
        public DbSet<UserReview> UserReviews { get; set; }

        // coach profile
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<CoachSpecialty> CoachSpecialties { get; set; }
        public DbSet<CoachLanguage> CoachLanguages { get; set; }
        public DbSet<CoachGymAccess> CoachGyms { get; set; }
        public DbSet<CoachAbsences> CoachAbsences { get; set; }

        public DbSet<Sport> Activities { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<Specialty> Specialties { get; set;}
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<CoachCustomSchedule> CoachCustomSchedules { get; set; }
        public DbSet<CoachEverydaySchedule> CoachEverydaySchedules { get; set; }

        // user notification
        public DbSet<Notification> Notifications { get; set; }

        // user notification
        public DbSet<UserNotification> UserNotifications { get; set; }

        // payment method
        public DbSet<PaymentMethod_Card> PaymentMehod_Cards { get; set; }

        // payment
        public DbSet<Payment> Payment { get; set; }

        // class
        public DbSet<IndividualClass> IndividualClasses { get; set; }
        public DbSet<IndividualClassDetails> IndividualClassDetails { get; set; }

        public DbSet<GroupClass> GroupClasses { get; set; }

        // Bookings
        public DbSet<IndividualBooking> IndividualBookings { get; set; }
        public DbSet<GroupBooking> GroupBookings { get; set; }

        //faqs
        public DbSet<FAQs> FAQs { get; set; }

        // setup configuration
        public DbSet<Surface> Surfaces { get; set; }
        public DbSet<TeamSize> Sizes { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<Area> Areas { get; set; }

        //legaldocu
        public DbSet<LegalDocument> LegalDocuments { get; set; }

        //banner
        public DbSet<Banner> Banners { get; set; }

        //play
        public DbSet<FreeGame> FreeGame { get; set; }
        public DbSet<PlayRequest> PlayRequest { get; set; }
        //public DbSet<GamePlayer> GamePlayers { get; set; }
        public DbSet<Promo> Promos { get; set; }

        //chat
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Playconversations> Playconversations { get; set; }
        public DbSet<ChatConversations> ChatConversations { get; set; }

        //comission
        public DbSet<CommissionPlay> ComissionPlays { get; set; }
        public DbSet<CommissionTrain> ComissionTrains { get; set; }

        //apple signin
        public DbSet<ExternalConfigKey> ExternalConfigKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Seed(MasterConf.HostURL);
        }
    }
}
