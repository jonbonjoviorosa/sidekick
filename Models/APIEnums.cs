namespace Sidekick.Model
{
    public enum EDevicePlatform
    {
        Others = -1,
        Web = 0,
        IOS = 1,
        Android = 2
    }

    public enum CoachingDay
    {
        SUNDAY = 0,
        MONDAY,
        TUESDAY,
        WEDNESDAY,
        THURSDAY,
        FRIDAY,
        SATURDAY
    }

    public enum UserRegistrationPlatform
    {
        API = 0,
        Google = 1,
        Facebook = 2,
        Apple
    }

    public enum Genders
    {
        Female = 0,
        Male = 1,
        Others = 2
    }

    public enum EVerificationType
    {
        RegisterAccount = 0,
        ForgotPassword = 1,
        ChangePassword = 2,
        ChangeEmail = 3,
        ChangePhoneNumber = 4
    }

    public enum UploadTypes
    {
        UploadBanner = 0,
        UploadIcon = 1,
        UploadBackgroundImage = 2,
        UploadProfileImage = 3,
        UploadDocument = 4
    }

    public enum APIResponseCode
    {
        UserIsNotYetVerified,
        UserHasNoAddress,
        IsSuperAdminUser,
        IsFacilityUser,
    }

    public enum EFacilityUserType
    {
        ADMIN = 1,
        OWNER,
        STAFF,
        ORGANIZER
    }

    public enum EUserType
    {
        NORMAL = 0,
        NORMALANDCOACH
    }

    public enum ETrainingType
    {
        INDIVIDUAL = 0,
        GROUP
    }

    public enum EAdminType
    {
        SUPERADMIN = 1,
        ADMINISTRATOR,
        ORGANIZER,
        STAFF
    }

    public enum PromoType
    {
        FIXED = 1,
        PERCENTAGE
    }

    public enum EAccountType
    {
        SUPERADMINUSER = 1,
        FACILITYUSER,
        MOBILEUSER
    }

    public enum EOperation
    {
        SELECT,
        INSERT,
        INSERT_UPDATE,
        UPDATE,
        DELETE,
        SENDEMAIL
    }

    public enum ETransaction
    {
        RUN,
        SUCCESS,
        FAILED
    }

    public enum EResponseAction
    {
        Unauthorized,
        InternalServerError,
        SendEmailFailed,
        SendEmailSuccess,
        UpdateSuccess,
        InsertSuccess,
        NotExist,
        RecordSuccess,
        PaymentMethod_CardExpired,
        NoTelRTranAttached,
        Error,
        PaymentNotDone,
        PaymentAlreadyDone,
        DeleteSuccess
    }

    public enum ELevel
    {
        Beginner = 0,
        Intermediate = 1,
        Advance = 2
    }

    public enum EDuring
    {
        Day = 0,
        Week = 1,
        Month = 2,
        Year = 3
    }


    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Cancelled = 3
    }


    public enum ENotificationType
    {
        Individualbooking = 0,
        Groupbooking = 1,
        PitchBooking = 2
    }

    public enum EBookingStatus
    {
        Pending = 0,
        Approved = 1,
        Declined = 2,
        Cancelled = 3,
        Confirmed = 4,
        Complete = 5
    }

    public enum EGamePlayerStatus
    {
        Pending = 0,
        Approved = 1,
        Declined = 2,
        Cancelled = 3,
        Confirmed = 4,
        Waiting = 5,
        CanInvite = 6,
        Complete = 7
    }

    public enum ELegalDocType
    {
        TermsAndCondition = 0,
        PrivacyPolicy = 1,
        CancellationPolicy = 2
    }
    public enum FilterType
    {
        Sport,
        Area,
        Price,
        TeamSize,
        Location,
        Surface,
        Gender,
        Activity,
        Level,
        GymCenter,
        Language,
        Date
    }

    public enum EBookingType
    {
        Individual,
        Group,
        Play
    }

    public enum Ratings
    {
        VeryBad = 1,
        Bad = 2,
        Good = 3,
        VerGood = 4,
        Excellent = 5,
    }

    public enum ReviewType
    {
        GroupClass = 0,
        IndividualClass,
        Pending
    }

    public enum INAPPNotificationTemplateType
    {
        PitchBookingCancellationFromCaptainMorethan24HoursToCaptain = 1,
        PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers = 2,
        PitchBookingCancellationFromPlayerMorethan24HoursToPlayer = 3,
        PitchBookingCancellationFromPlayerMorethan24HoursToCaptain = 4,
        PitchBookingCancellationFromCaptainLessthan24HoursToCaptain = 5,
        PitchBookingCancellationFromCaptainLessthan24HoursToAllPlayers = 6,
        PitchBookingCancellationFromPlayerLessthan24HoursToPlayer = 7,
        PitchBookingCancellationFromPlayerLessthan24HoursToCaptain = 8,
        PitchBookingCancellationFromFacilityMorethan24HoursToCaptain = 9,
        PitchBookingCancellationFromFacilityMorethan24HoursToAllPlayers = 10,
        PitchBookingCancellationFromFacilityLessthan24HoursToCaptain = 11,
        PitchBookingCancellationFromFacilityLessthan24HoursToAllPlayers = 12,
        IndividualCoachingBookingConfirmationToPlayer = 13,
        GroupCoachingBookingConfirmationToPlayer = 14,
        CancellationCoachingFromUserMorethan24HoursToPlayer = 15,
        CancellationCoachingFromUserLessthan24HoursToPlayer = 16,
        CancellationCoachingFromCoachMorethan24HoursToPlayer = 17,
        CancellationCoachingFromCoachLessthan24HoursToPlayer = 18,
        PitchBookingConfirmationToCaptain = 19,
        PitchBookingConfirmationToPlayer = 20,
        IndividualCoachingRequestToCoach = 21,
        GroupCoachingBookingConfirmationToCoach = 22,
        IndividualCoachingCancellationLessthan24HoursToCoach = 23,
        IndividualCoachingCancellationMorethan24HoursToCoach = 24,
        GroupCoachingCancellationLessthan24HoursToCoach = 25,
        GroupCoachingCancellationMorethan24HoursToCoach = 26,
        ShareEventToPlayer = 27,
        CaptainAcceptsTheRequestToPlayer = 28,
        OneSpotIsFreeFromWaitingListToPlayer = 29,
        PitchBookingConfirmationToFacility =30,
        PitchBookingCancellationFromCaptainMorethan24HoursToFacility =31,
        PitchBookingCancellationFromPlayerMorethan24HoursToFacility = 32
    }
}
