using Sidekick.Model;
using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Helpers
{
    public static class EmailTemplatesHelper
    {
        public static int IndividualCoachingBookingConfirmation(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.IndividualCoachingBookingConfirmationEmailSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.IndividualCoachingBookingConfirmation,
                emailTemplate.UserName,
                emailTemplate.Activity,
                emailTemplate.CoachName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.TotalAmount, emailTemplate.ServiceFees,
                emailTemplate.TotalAmount);
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }


        public static int GroupCoachingBookingConfirmation(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.GroupCoachingBookingConfirmationEmailSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.GroupCoachingBookingConfirmation,
                emailTemplate.UserName,
                emailTemplate.Activity,
                emailTemplate.CoachName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PriceCoaching, emailTemplate.ServiceFees,
                emailTemplate.TotalAmount);
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int CancellationFromUserMorethan24HoursEmailSubject(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.CancellationFromUserMorethan24HoursEmailSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.CancellationFromUserMorethan24Hours,
                emailTemplate.UserName,
                emailTemplate.Type,
                emailTemplate.Activity,
                emailTemplate.CoachName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PriceCoaching,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount);
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }
        public static int CancellationFromUserLessthan24Hours(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.CancellationFromUserLessthan24HoursEmailSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.CancellationFromUserLessthan24Hours,
                emailTemplate.UserName,
                emailTemplate.Type,
                emailTemplate.Activity,
                emailTemplate.CoachName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PriceCoaching,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount);
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }
        //CancellationFromCoachMorethan24Hours

        public static int CancellationFromCoachMorethan24Hours(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.CancellationFromCoachMorethan24HoursEmailSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.CancellationFromCoachMorethan24Hours,
                emailTemplate.UserName,
                emailTemplate.Type,
                emailTemplate.Activity,
                emailTemplate.CoachName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PriceCoaching,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount);
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int CancellationFromCoachLessthan24Hours(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.CancellationFromCoachLessthan24HoursEmailSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.CancellationFromCoachLessthan24Hours,
                emailTemplate.UserName,
                emailTemplate.Type,
                emailTemplate.Activity,
                emailTemplate.CoachName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PriceCoaching,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount);
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PitchBookingConfirmationToCaptain(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.PitchBookingConfirmationEmailSubjectToCaptain;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PitchBookingConfirmationToCaptain,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePitch,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount,
                emailTemplate.PricePerPlayer);
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int SpotBookingConfirmation(APIConfigurationManager APIConfig, APIConfigurationManager _conf, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = _conf.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.SpotBookingConfirmationEmailSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.SpotBookingConfirmation,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePerPlayer,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount
                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PitchBookingCancellationFromUserhMorethan24HoursToCaptain(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.PitchBookingCancellationFromUserMorethan24HoursEmailToCaptainSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromUserMorethan24HoursToCaptain,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePitch,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount,
                emailTemplate.PricePerPlayer
                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PitchBookingCancellationFromUserMorethan24HoursToPlayer(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.PitchBookingCancellationFromUserMorethan24HoursEmailToPlayerSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromUserMorethan24HoursToPlayer,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePerPlayer,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount

                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PitchBookingCancellationFromUserLessthan24HoursToCaptain(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.PitchBookingCancellationFromUserLessthan24HoursEmailToCaptainSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromUserLessthan24HoursToCaptain,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePitch,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount,
                emailTemplate.PricePerPlayer
                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PitchBookingCancellationFromUserLessthan24HoursToPlayer(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.PitchBookingCancellationFromUserLessthan24HoursEmailToPlayerSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromUserLessthan24HoursToPlayer,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePerPlayer,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount

                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }


        public static int PitchBookingCancellationFromFacilityMorethan24HoursToCaptain(APIConfigurationManager APIConfig, APIConfigurationManager _conf, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = _conf.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = string.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromFacilityMorethan24HoursEmailToCaptainSubject, emailTemplate.FacilityName);
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromFacilityMorethan24HoursToCaptain,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePitch,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount,
                emailTemplate.PricePerPlayer
                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PitchBookingCancellationFromFacilityMorethan24HoursToPlayer(APIConfigurationManager APIConfig, APIConfigurationManager _conf, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = _conf.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = string.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromFacilityMorethan24HoursEmailToPlayerSubject, emailTemplate.FacilityName);
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromFacilityMorethan24HoursToPlayer,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePerPlayer,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount
                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PitchBookingCancellationFromFacilityLessthan24HoursToCaptain(APIConfigurationManager APIConfig, APIConfigurationManager _conf, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = _conf.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = string.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromFacilityLessthan24HoursEmailToCaptainSubject, emailTemplate.FacilityName);
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromFacilityLessthan24HoursToCaptain,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePitch,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount,
                emailTemplate.PricePerPlayer
                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PitchBookingCancellationFromFacilityLessthan24HoursToPlayer(APIConfigurationManager APIConfig, APIConfigurationManager _conf, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = _conf.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = string.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromFacilityLessthan24HoursEmailToPlayerSubject, emailTemplate.FacilityName);
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PitchBookingCancellationFromFacilityLessthan24HoursToPlayer,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePerPlayer,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount
                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PaymentFailedForPlay(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.PaymentFailedForPlayEmailSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PaymentFailedForPlay,
                emailTemplate.UserName,
                emailTemplate.Sport,
                emailTemplate.FacilityName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PricePerPlayer,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount
                );
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }

        public static int PaymentFailedForTrain(APIConfigurationManager APIConfig,ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.PaymentFailedForTrainEmailSubject;
            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.PaymentFailedForTrain,
                emailTemplate.UserName,
                emailTemplate.Type,
                emailTemplate.Activity,
                emailTemplate.CoachName,
                emailTemplate.BookingDate.ToString("dd-MM-yyyy"),
                emailTemplate.BookingTime,
                emailTemplate.Location,
                emailTemplate.PriceCoaching,
                emailTemplate.ServiceFees,
                emailTemplate.TotalAmount);
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return EmailHelper.SendEmailByEmailAddress(emailTemplate.EmailTo, EmailParam, loggerManager);
        }


        public static string SetEmailHtmlTemplate(APIConfigurationManager _conf, string Body, ILoggerManager loggerManager, bool isBookSessionRequired = true)
        {
            loggerManager.LogInfo("Start read html template file");
            try
            {
                var htmlFile = System.IO.File.ReadAllText(_conf.MailConfig.NotificationTemplatePath);
                loggerManager.LogInfo("Start read html template file found");
                if (!string.IsNullOrWhiteSpace(htmlFile))
                {
                    loggerManager.LogInfo("Start read html template file found with content and replace content");

                    Body = htmlFile.Replace("##EmailTemplateContent##", Body);
                    Body = Body.Replace("##BookSessionRedirectURL##", _conf.MailConfig.BookSessionBaseUrl);
                    if (isBookSessionRequired)
                    {
                        Body = Body.Replace("##HideBookSession##", "");
                    }
                    else
                    {
                        Body = Body.Replace("##HideBookSession##", "display:none;");
                    }
                }
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo("Error while read html template file" + ex.Message);
            }
            return Body;
        }

        public static async Task<int> FacilitySendContactMessageToPlayer(APIConfigurationManager APIConfig, ILoggerManager loggerManager, BookingNotificationCommonTemplate emailTemplate)
        {
            var EmailParam = APIConfig.MailConfig;

            EmailParam.To = emailTemplate.EmailTo;
            EmailParam.Subject = APIConfig.BookingNotificationConfig.FacilitySendContactMessageToPlayerSubject;
            EmailParam.Body = emailTemplate.Message;
            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(APIConfig, EmailParam.Body, loggerManager);
            return await EmailHelper.SendEmailByEmailAddressAsync(emailTemplate.EmailTo, EmailParam, loggerManager);
        }
    }


}
