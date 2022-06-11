using System.Collections.Generic;

namespace Sidekick.Api.Helpers
{
    public class APIConfigurationManager
    {
        public DataStr DataStrings { get; set; }
        public Token TokenKeys { get; set; }
        public PushNotificationConfig PNConfig { get; set; }
        public SMTPConfig MailConfig { get; set; }
        public SideKickAdminEmail SideKickAdminEmailConfig { get; set; }
        public string HostURL { get; set; }
        public string DefaultClientSite { get; set; }
        public string MapUrl { get; set; }
        public SmsParameter SmsConfig { get; set; }
        public MessageConfigurations MsgConfigs { get; set; }
        public PaymentConfig PaymentConfig { get; set; }
        public PlayRequestConfig PlayRequestConfig { get; set; }
        public BookingNotificationConfig BookingNotificationConfig { get; set; }

        public PushNotificationTemplateConfig pushNotificationTemplateConfig { get; set; }
        public SMSTemplate SMSTemplateConfig { get; set; }
        public INAPPNotificationTemplateConfig INAPPNotificationTemplateConfig { get; set; }

        public AppleSignInConfiguration AppleSignInConfig { get; set; }
    }

    public class DataStr
    {
        public string ConnStr { get; set; }
    }

    public class Token
    {
        public string Key { get; set; }
        public double Exp { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
    }

    public class PushNotificationConfig
    {
        public string FirebaseServerKey { get; set; }
        public string FirebaseSenderId { get; set; }
        public string FirebaseURL { get; set; }
        public string FireBLink { get; set; }
        public string IOSFireBKey { get; set; }
        public string AndroidFireBKey { get; set; }
        public string IOSSenderID { get; set; }
        public string AndroidSenderID { get; set; }
    }

    public class PushNotificationTemplateConfig
    {
        public string BookingStartingPlaySubject { get; set; }
        public string BookingStartingPlay { get; set; }

        public string BookingStartingTrainSubject { get; set; }
        public string BookingStartingTrain { get; set; }

        public string PaymentFailPlaySubject { get; set; }
        public string PaymentFailPlay { get; set; }

        public string PaymentFailTrainSubject { get; set; }
        public string PaymentFailTrain { get; set; }

        public string ShareEventSubject { get; set; }
        public string ShareEvent { get; set; }

        public string CaptainAcceptsTheRequestSubject { get; set; }
        public string CaptainAcceptsTheRequest { get; set; }

        public string OneSpotIsFreeFromWaitingListSubject { get; set; }
        public string OneSpotIsFreeFromWaitingList { get; set; }

    }

    public class SMTPConfig
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RegistrationLink { get; set; }
        public int Port { get; set; }
        public bool EnableSSL { get; set; }

        public IEnumerable<string> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public string NotificationTemplatePath { get; set; }
        public string BookSessionBaseUrl { get; set; }
    }

    public class SideKickAdminEmail
    {
        public string Subject { get; set; }
        public string Email { get; set; }
    }

    public class SmsParameter
    {
        public string Endpoint { get; set; }

        // required
        public string Action { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }

        // optional
        public int Maxsplit { get; set; }
        public string Scheduledatetime { get; set; }
        public string Optout { get; set; }
        public string Api { get; set; }
        public string Apireply { get; set; }

        public bool IsEnable { get; set; }

        public SmsParameter()
        {
            // TODO : initialize optional parameters?
        }
    }

    public class MessageConfigurations
    {
        public string RegisterMobileUserEmailSubject { get; set; }
        public string RegisterMobileUser { get; set; }
        public string RegisterFacilityUserEmailSubject { get; set; }
        public string RegisterFacilityUser { get; set; }
        public string ForgotPasswordEmailSubject { get; set; }
        public string ForgotPassword { get; set; }
        public string ChangeNumber { get; set; }
        public string ResendCodeUserEmailSubject { get; set; }
        public string ResendCode { get; set; }
        public string WelcomeMsgEmailSubject { get; set; }
        public string WelcomeMsg { get; set; }
        public string OrderPlaced { get; set; }
        public string OrderCompleted { get; set; }
    }

    public class PaymentConfig
    {
        public string ClientId { get; set; }
        public string ApiKey { get; set; }
        public string RemoteKey { get; set; }
        public string CreditedId { get; set; }
        public string WalletId { get; set; }
        public string BaseUrl { get; set; }
        public string ReturlUrl { get; set; }
        public string ReturnAuthURL { get; set; }
        public string ReturnDeclinedURL { get; set; }
        public string ReturnCancelledURL { get; set; }

    }

    public class AppleSignInConfiguration
    {
        public string ClientId { get; set; }
        public string TeamId { get; set; }
        public string KeyId { get; set; }
        public string PrivateKey { get; set; }
        public string ValidationUrl { get; set; }
        public string ValidIssuerUrl { get; set; }
        public string PublicKeysUrl { get; set; }
    }

    public class PlayRequestConfig
    {
        public string RequestMessage { get; set; }
        public string BookingConfirmation { get; set; }
        public string RequestConfirmation { get; set; }
    }

    public class BookingNotificationConfig
    {
        public string IndividualCoachingBookingConfirmationEmailSubject { get; set; }
        public string IndividualCoachingBookingConfirmation { get; set; }

        public string GroupCoachingBookingConfirmationEmailSubject { get; set; }
        public string GroupCoachingBookingConfirmation { get; set; }

        public string CancellationFromUserMorethan24HoursEmailSubject { get; set; }
        public string CancellationFromUserMorethan24Hours { get; set; }

        public string CancellationFromUserLessthan24HoursEmailSubject { get; set; }
        public string CancellationFromUserLessthan24Hours { get; set; }

        public string CancellationFromCoachMorethan24HoursEmailSubject { get; set; }
        public string CancellationFromCoachMorethan24Hours { get; set; }

        public string CancellationFromCoachLessthan24HoursEmailSubject { get; set; }
        public string CancellationFromCoachLessthan24Hours { get; set; }

        public string PitchBookingConfirmationEmailSubjectToCaptain { get; set; }
        public string PitchBookingConfirmationToCaptain { get; set; }

        public string SpotBookingConfirmationEmailSubject { get; set; }
        public string SpotBookingConfirmation { get; set; }

        public string PitchBookingCancellationFromUserMorethan24HoursEmailToCaptainSubject { get; set; }
        public string PitchBookingCancellationFromUserMorethan24HoursToCaptain { get; set; }

        public string PitchBookingCancellationFromUserMorethan24HoursEmailToPlayerSubject { get; set; }

        public string PitchBookingCancellationFromUserMorethan24HoursToPlayer { get; set; }

        public string PitchBookingCancellationFromUserLessthan24HoursEmailToCaptainSubject { get; set; }

        public string PitchBookingCancellationFromUserLessthan24HoursToCaptain { get; set; }


        public string PitchBookingCancellationFromUserLessthan24HoursEmailToPlayerSubject { get; set; }
        public string PitchBookingCancellationFromUserLessthan24HoursToPlayer { get; set; }


        public string PitchBookingCancellationFromFacilityMorethan24HoursEmailToCaptainSubject { get; set; }
        public string PitchBookingCancellationFromFacilityMorethan24HoursToCaptain { get; set; }


        public string PitchBookingCancellationFromFacilityMorethan24HoursEmailToPlayerSubject { get; set; }
        public string PitchBookingCancellationFromFacilityMorethan24HoursToPlayer { get; set; }

        public string PitchBookingCancellationFromFacilityLessthan24HoursEmailToCaptainSubject { get; set; }
        public string PitchBookingCancellationFromFacilityLessthan24HoursToCaptain { get; set; }

        public string PitchBookingCancellationFromFacilityLessthan24HoursEmailToPlayerSubject { get; set; }
        public string PitchBookingCancellationFromFacilityLessthan24HoursToPlayer { get; set; }


        public string PaymentFailedForPlayEmailSubject { get; set; }
        public string PaymentFailedForPlay { get; set; }

        public string PaymentFailedForTrainEmailSubject { get; set; }
        public string PaymentFailedForTrain { get; set; }

        public string FacilitySendContactMessageToPlayerSubject { get; set; }
    }
    public class SMSTemplate
    {
        public string OTPForRegistration { get; set; }
    }

    public class INAPPNotificationTemplateConfig
    {
        public string PitchBookingCancellationFromCaptainMorethan24HoursToCaptain { get; set; }
        public string PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers { get; set; }

        public string PitchBookingCancellationFromPlayerMorethan24HoursToPlayer { get; set; }

        public string PitchBookingCancellationFromPlayerMorethan24HoursToCaptain { get; set; }

        public string PitchBookingCancellationFromCaptainLessthan24HoursToCaptain { get; set; }

        public string PitchBookingCancellationFromCaptainLessthan24HoursToAllPlayers { get; set; }

        public string PitchBookingCancellationFromPlayerLessthan24HoursToPlayer { get; set; }

        public string PitchBookingCancellationFromPlayerLessthan24HoursToCaptain { get; set; }

        public string PitchBookingCancellationFromFacilityMorethan24HoursToCaptain { get; set; }

        public string PitchBookingCancellationFromFacilityMorethan24HoursToAllPlayers { get; set; }

        public string PitchBookingCancellationFromFacilityLessthan24HoursToCaptain { get; set; }

        public string PitchBookingCancellationFromFacilityLessthan24HoursToAllPlayers { get; set; }

        public string IndividualCoachingBookingConfirmationToPlayer { get; set; }

        public string GroupCoachingBookingConfirmationToPlayer { get;set;}

        public string CancellationCoachingFromUserMorethan24HoursToPlayer { get; set; }
        public string CancellationCoachingFromUserLessthan24HoursToPlayer { get; set; }

        public string CancellationCoachingFromCoachMorethan24HoursToPlayer { get; set; }

        public string CancellationCoachingFromCoachLessthan24HoursToPlayer { get; set; }

        public string PitchBookingConfirmationToCaptain { get; set; }

        public string PitchBookingConfirmationToPlayer { get; set; }

        public string IndividualCoachingRequestToCoach { get; set; }

        public string GroupCoachingBookingConfirmationToCoach { get; set; }

        public string IndividualCoachingCancellationLessthan24HoursToCoach { get; set; }
        public string IndividualCoachingCancellationMorethan24HoursToCoach { get; set; }

        public string GroupCoachingCancellationLessthan24HoursToCoach { get; set; }

        public string GroupCoachingCancellationMorethan24HoursToCoach { get; set; }

        public string ShareEventToPlayer { get; set; }

        public string CaptainAcceptsTheRequestToPlayer { get; set; }

        public string OneSpotIsFreeFromWaitingListToPlayer { get; set; }

        public string PitchBookingConfirmationToFacility { get; set; }

        public string PitchBookingCancellationFromCaptainMorethan24HoursToFacility { get; set; }

        public string PitchBookingCancellationFromPlayerMorethan24HoursToFacility { get; set; }
    }
}
