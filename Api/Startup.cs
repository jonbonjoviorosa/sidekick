using System;
using System.IO;
using System.Text;
using Sidekick.Api.DataAccessLayer;
using Sidekick.Api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sidekick.Api.Handlers;
using Sidekick.Api.Service;

namespace Sidekick.Api
{
    public class Startup
    {
        APIConfigurationManager masterConfig = new APIConfigurationManager();
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region "Database Entities"
            services.AddDbContext<APIDBContext>(options => options.UseMySql(Configuration["Data:DataConnStr"], ServerVersion.AutoDetect(Configuration["Data:DataConnStr"]), null), ServiceLifetime.Scoped);
            #endregion "Database Entities"

            services.AddDALContexts(); //Entities
            services.AddHandlerContexts(); // Handlers
            services.AddServiceContexts(); // Third Party Services

            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IMainHttpClient, MainHttpClient>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddHttpContextAccessor();
            #region "ConfigurationBuilder"
            //build the master configuration
            masterConfig = new APIConfigurationManager
            {
                DataStrings = new DataStr
                {
                    ConnStr = Configuration["Data:ConnStr"]
                },
                TokenKeys = new Token
                {
                    Key = Configuration["Tokens:Key"],
                    Exp = Convert.ToDouble(Configuration["Tokens:Exp"]),
                    Audience = Configuration["Link:Audience"],
                    Issuer = Configuration["Link:Issuer"]
                },
                PNConfig = new PushNotificationConfig
                {
                    FirebaseURL = Configuration["PushNotification:FirebaseURL"],
                    FirebaseSenderId = Configuration["PushNotification:FirebaseSenderId"],
                    FirebaseServerKey = Configuration["PushNotification:FirebaseServerKey"],
                    FireBLink = Configuration["PushNotification:FireBLink"],
                    IOSFireBKey = Configuration["PushNotification:iOSFireBKey"],
                    AndroidFireBKey = Configuration["PushNotification:androidFireBKey"],
                    IOSSenderID = Configuration["PushNotification:iOSSenderID"],
                    AndroidSenderID = Configuration["PushNotification:androidSenderID"]
                },
                MailConfig = new SMTPConfig
                {
                    Server = Configuration["Mail:Server"],
                    Password = Configuration["Mail:Password"],
                    Username = Configuration["Mail:Username"],
                    RegistrationLink = Configuration["Mail:RegistrationLink"],
                    Port = Convert.ToInt32(Configuration["Mail:Port"]),
                    EnableSSL = Convert.ToBoolean(Configuration["Mail:EnableSSL"]),
                    NotificationTemplatePath = Configuration["Mail:NotificationTemplatePath"],
                    BookSessionBaseUrl = Configuration["Mail:BookSessionBaseUrl"]
                },
                SideKickAdminEmailConfig = new SideKickAdminEmail
                {
                    Subject = Configuration["SideKickAdminMail:Subject"],
                    Email = Configuration["SideKickAdminMail:Email"]
                },
                SmsConfig = new SmsParameter
                {
                    Endpoint = Configuration["Sms:Endpoint"],
                    Action = Configuration["Sms:Action"],
                    User = Configuration["Sms:User"],
                    Password = Configuration["Sms:Password"],
                    From = Configuration["Sms:From"],
                    To = Configuration["Sms:To"],
                    Text = Configuration["Sms:Text"],
                    IsEnable = !string.IsNullOrWhiteSpace(Configuration["Sms:IsEnable"]) ? Convert.ToBoolean(Configuration["Sms:IsEnable"]) : false
                },
                MsgConfigs = new MessageConfigurations
                {
                    RegisterMobileUserEmailSubject = Configuration["MsgConfigs:RegisterMobileUserEmailSubject"],
                    RegisterMobileUser = Configuration["MsgConfigs:RegisterMobileUser"],
                    RegisterFacilityUserEmailSubject = Configuration["MsgConfigs:RegisterFacilityUserEmailSubject"],
                    RegisterFacilityUser = Configuration["MsgConfigs:RegisterFacilityUser"],
                    ForgotPasswordEmailSubject = Configuration["MsgConfigs:ForgotPasswordEmailSubject"],
                    ForgotPassword = Configuration["MsgConfigs:ForgotPassword"],
                    ChangeNumber = Configuration["MsgConfigs:ChangeNumber"],
                    ResendCodeUserEmailSubject = Configuration["MsgConfigs:ResendCodeUserEmailSubject"],
                    ResendCode = Configuration["MsgConfigs:ResendCode"],
                    WelcomeMsgEmailSubject = Configuration["MsgConfigs:WelcomeMsgEmailSubject"],
                    WelcomeMsg = Configuration["MsgConfigs:WelcomeMsg"],
                    OrderPlaced = Configuration["MsgConfigs:OrderPlaced"],
                    OrderCompleted = Configuration["MsgConfigs:OrderCompleted"]
                },
                PaymentConfig = new PaymentConfig
                {
                    ClientId = Configuration["PaymentConfig:ClientId"],
                    ApiKey = Configuration["PaymentConfig:ApiKey"],
                    RemoteKey = Configuration["PaymentConfig:RemoteKey"],
                    CreditedId = Configuration["PaymentConfig:CreditedId"],
                    WalletId = Configuration["PaymentConfig:WalletId"],
                    BaseUrl = Configuration["PaymentConfig:BaseUrl"],
                    ReturlUrl = Configuration["PaymentConfig:PaymentReturnUrl"],
                    ReturnAuthURL = Configuration["PaymentConfig:ReturnAuthURL"],
                    ReturnCancelledURL = Configuration["PaymentConfig:ReturnDeclinedURL"],
                    ReturnDeclinedURL = Configuration["PaymentConfig:ReturnCancelledURL"]
                },
                PlayRequestConfig = new PlayRequestConfig
                {
                    RequestMessage = Configuration["PlayRequestConfig:RequestMessage"],
                    BookingConfirmation = Configuration["PlayRequestConfig:BookingConfirmation"],
                    RequestConfirmation = Configuration["PlayRequestConfig:RequestConfirmation"]
                },
                BookingNotificationConfig = new BookingNotificationConfig
                {
                    IndividualCoachingBookingConfirmation = Configuration["BookingNotificationConfig:IndividualCoachingBookingConfirmation"],
                    IndividualCoachingBookingConfirmationEmailSubject = Configuration["BookingNotificationConfig:IndividualCoachingBookingConfirmationEmailSubject"],

                    GroupCoachingBookingConfirmation = Configuration["BookingNotificationConfig:GroupCoachingBookingConfirmation"],
                    GroupCoachingBookingConfirmationEmailSubject = Configuration["BookingNotificationConfig:GroupCoachingBookingConfirmationEmailSubject"],

                    CancellationFromUserMorethan24Hours = Configuration["BookingNotificationConfig:CancellationFromUserMorethan24Hours"],
                    CancellationFromUserMorethan24HoursEmailSubject = Configuration["BookingNotificationConfig:CancellationFromUserMorethan24HoursEmailSubject"],

                    CancellationFromUserLessthan24Hours = Configuration["BookingNotificationConfig:CancellationFromUserLessthan24Hours"],
                    CancellationFromUserLessthan24HoursEmailSubject = Configuration["BookingNotificationConfig:CancellationFromUserLessthan24HoursEmailSubject"],

                    PitchBookingConfirmationToCaptain = Configuration["BookingNotificationConfig:PitchBookingConfirmationToCaptain"],
                    PitchBookingConfirmationEmailSubjectToCaptain = Configuration["BookingNotificationConfig:PitchBookingConfirmationEmailToCaptainSubject"],

                    SpotBookingConfirmation = Configuration["BookingNotificationConfig:SpotBookingConfirmation"],
                    SpotBookingConfirmationEmailSubject = Configuration["BookingNotificationConfig:SpotBookingConfirmationEmailSubject"],

                    PitchBookingCancellationFromUserMorethan24HoursToCaptain = Configuration["BookingNotificationConfig:PitchBookingCancellationFromUserMorethan24HoursToCaptain"],
                    PitchBookingCancellationFromUserMorethan24HoursEmailToCaptainSubject = Configuration["BookingNotificationConfig:PitchBookingCancellationFromUserMorethan24HoursEmailToCaptainSubject"],

                    CancellationFromCoachMorethan24Hours = Configuration["BookingNotificationConfig:CancellationFromCoachMorethan24Hours"],
                    CancellationFromCoachMorethan24HoursEmailSubject = Configuration["BookingNotificationConfig:CancellationFromCoachMorethan24HoursEmailSubject"],

                    CancellationFromCoachLessthan24Hours = Configuration["BookingNotificationConfig:CancellationFromCoachLessthan24Hours"],
                    CancellationFromCoachLessthan24HoursEmailSubject = Configuration["BookingNotificationConfig:CancellationFromCoachLessthan24HoursEmailSubject"],

                    PitchBookingCancellationFromUserMorethan24HoursToPlayer = Configuration["BookingNotificationConfig:PitchBookingCancellationFromUserMorethan24HoursToPlayer"],
                    PitchBookingCancellationFromUserMorethan24HoursEmailToPlayerSubject = Configuration["BookingNotificationConfig:PitchBookingCancellationFromUserMorethan24HoursEmailToPlayerSubject"],

                    PitchBookingCancellationFromUserLessthan24HoursToCaptain = Configuration["BookingNotificationConfig:PitchBookingCancellationFromUserLessthan24HoursToCaptain"],
                    PitchBookingCancellationFromUserLessthan24HoursEmailToCaptainSubject = Configuration["BookingNotificationConfig:PitchBookingCancellationFromUserLessthan24HoursEmailToCaptainSubject"],

                    PitchBookingCancellationFromUserLessthan24HoursToPlayer = Configuration["BookingNotificationConfig:PitchBookingCancellationFromUserLessthan24HoursToPlayer"],
                    PitchBookingCancellationFromUserLessthan24HoursEmailToPlayerSubject = Configuration["BookingNotificationConfig:PitchBookingCancellationFromUserLessthan24HoursEmailToPlayerSubject"],

                    PitchBookingCancellationFromFacilityMorethan24HoursToCaptain = Configuration["BookingNotificationConfig:PitchBookingCancellationFromFacilityMorethan24HoursToCaptain"],
                    PitchBookingCancellationFromFacilityMorethan24HoursEmailToCaptainSubject = Configuration["BookingNotificationConfig:PitchBookingCancellationFromFacilityMorethan24HoursEmailToCaptainSubject"],

                    PitchBookingCancellationFromFacilityMorethan24HoursToPlayer = Configuration["BookingNotificationConfig:PitchBookingCancellationFromFacilityMorethan24HoursToPlayer"],
                    PitchBookingCancellationFromFacilityMorethan24HoursEmailToPlayerSubject = Configuration["BookingNotificationConfig:PitchBookingCancellationFromFacilityMorethan24HoursEmailToPlayerSubject"],

                    PitchBookingCancellationFromFacilityLessthan24HoursToCaptain = Configuration["BookingNotificationConfig:PitchBookingCancellationFromFacilityLessthan24HoursToCaptain"],
                    PitchBookingCancellationFromFacilityLessthan24HoursEmailToCaptainSubject = Configuration["BookingNotificationConfig:PitchBookingCancellationFromFacilityLessthan24HoursEmailToCaptainSubject"],

                    PitchBookingCancellationFromFacilityLessthan24HoursToPlayer = Configuration["BookingNotificationConfig:PitchBookingCancellationFromFacilityLessthan24HoursToPlayer"],
                    PitchBookingCancellationFromFacilityLessthan24HoursEmailToPlayerSubject = Configuration["BookingNotificationConfig:PitchBookingCancellationFromFacilityLessthan24HoursEmailToPlayerSubject"],

                    PaymentFailedForPlay = Configuration["BookingNotificationConfig:PaymentFailedForPlay"],
                    PaymentFailedForPlayEmailSubject = Configuration["BookingNotificationConfig:PaymentFailedForPlayEmailSubject"],

                    PaymentFailedForTrain = Configuration["BookingNotificationConfig:PaymentFailedForTrain"],
                    PaymentFailedForTrainEmailSubject = Configuration["BookingNotificationConfig:PaymentFailedForTrainEmailSubject"],
                    FacilitySendContactMessageToPlayerSubject = Configuration["BookingNotificationConfig:FacilitySendContactMessageToPlayerSubject"]

                }
                ,
                pushNotificationTemplateConfig = new PushNotificationTemplateConfig
                {
                    BookingStartingPlay = Configuration["PushNotificationTemplateConfig:BookingStartingPlay"],
                    BookingStartingPlaySubject = Configuration["PushNotificationTemplateConfig:BookingStartingPlaySubject"],
                    BookingStartingTrain = Configuration["PushNotificationTemplateConfig:BookingStartingTrain"],
                    BookingStartingTrainSubject = Configuration["PushNotificationTemplateConfig:BookingStartingTrainSubject"],
                    PaymentFailPlay = Configuration["PushNotificationTemplateConfig:PaymentFailPlay"],
                    PaymentFailPlaySubject = Configuration["PushNotificationTemplateConfig:PaymentFailPlaySubject"],
                    PaymentFailTrain = Configuration["PushNotificationTemplateConfig:PaymentFailTrain"],
                    PaymentFailTrainSubject = Configuration["PushNotificationTemplateConfig:PaymentFailTrainSubject"],
                    ShareEvent = Configuration["PushNotificationTemplateConfig:ShareEvent"],
                    ShareEventSubject = Configuration["PushNotificationTemplateConfig:ShareEventSubject"],
                    CaptainAcceptsTheRequest = Configuration["PushNotificationTemplateConfig:CaptainAcceptsTheRequest"],
                    CaptainAcceptsTheRequestSubject = Configuration["PushNotificationTemplateConfig:CaptainAcceptsTheRequestSubject"],
                    OneSpotIsFreeFromWaitingList = Configuration["PushNotificationTemplateConfig:OneSpotIsFreeFromWaitingList"],
                    OneSpotIsFreeFromWaitingListSubject = Configuration["PushNotificationTemplateConfig:OneSpotIsFreeFromWaitingListSubject"],
                },
                SMSTemplateConfig = new SMSTemplate
                {
                    OTPForRegistration = Configuration["SMSTemplate:OTPForRegistration"]
                },
                INAPPNotificationTemplateConfig = new INAPPNotificationTemplateConfig
                {
                    PitchBookingCancellationFromCaptainMorethan24HoursToCaptain = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromCaptainMorethan24HoursToCaptain"],
                    PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers"],
                    PitchBookingCancellationFromPlayerMorethan24HoursToPlayer = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromPlayerMorethan24HoursToPlayer"],
                    PitchBookingCancellationFromPlayerMorethan24HoursToCaptain = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromPlayerMorethan24HoursToCaptain"],
                    PitchBookingCancellationFromCaptainLessthan24HoursToAllPlayers = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromCaptainLessthan24HoursToAllPlayers"],
                    PitchBookingCancellationFromCaptainLessthan24HoursToCaptain = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromCaptainLessthan24HoursToCaptain"],
                    PitchBookingCancellationFromPlayerLessthan24HoursToCaptain = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromPlayerLessthan24HoursToCaptain"],
                    PitchBookingCancellationFromPlayerLessthan24HoursToPlayer = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromPlayerLessthan24HoursToPlayer"],
                    PitchBookingCancellationFromFacilityMorethan24HoursToCaptain = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromFacilityMorethan24HoursToCaptain"],
                    PitchBookingCancellationFromFacilityMorethan24HoursToAllPlayers = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromFacilityMorethan24HoursToAllPlayers"],
                    PitchBookingCancellationFromFacilityLessthan24HoursToCaptain = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromFacilityLessthan24HoursToCaptain"],
                    PitchBookingCancellationFromFacilityLessthan24HoursToAllPlayers = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromFacilityLessthan24HoursToAllPlayers"],
                    IndividualCoachingBookingConfirmationToPlayer = Configuration["INAPPNotificationTemplateConfig:IndividualCoachingBookingConfirmationToPlayer"],
                    GroupCoachingBookingConfirmationToPlayer = Configuration["INAPPNotificationTemplateConfig:GroupCoachingBookingConfirmationToPlayer"],
                    CancellationCoachingFromUserMorethan24HoursToPlayer = Configuration["INAPPNotificationTemplateConfig:CancellationCoachingFromUserMorethan24HoursToPlayer"],
                    CancellationCoachingFromUserLessthan24HoursToPlayer = Configuration["INAPPNotificationTemplateConfig:CancellationCoachingFromUserLessthan24HoursToPlayer"],
                    CancellationCoachingFromCoachMorethan24HoursToPlayer = Configuration["INAPPNotificationTemplateConfig:CancellationCoachingFromCoachMorethan24HoursToPlayer"],
                    CancellationCoachingFromCoachLessthan24HoursToPlayer = Configuration["INAPPNotificationTemplateConfig:CancellationCoachingFromCoachLessthan24HoursToPlayer"],
                    PitchBookingConfirmationToCaptain = Configuration["INAPPNotificationTemplateConfig:PitchBookingConfirmationToCaptain"],
                    PitchBookingConfirmationToPlayer = Configuration["INAPPNotificationTemplateConfig:PitchBookingConfirmationToPlayer"],
                    IndividualCoachingRequestToCoach = Configuration["INAPPNotificationTemplateConfig:IndividualCoachingRequestToCoach"],
                    GroupCoachingBookingConfirmationToCoach = Configuration["INAPPNotificationTemplateConfig:GroupCoachingBookingConfirmationToCoach"],
                    IndividualCoachingCancellationLessthan24HoursToCoach = Configuration["INAPPNotificationTemplateConfig:IndividualCoachingCancellationLessthan24HoursToCoach"],
                    IndividualCoachingCancellationMorethan24HoursToCoach = Configuration["INAPPNotificationTemplateConfig:IndividualCoachingCancellationMorethan24HoursToCoach"],
                    GroupCoachingCancellationLessthan24HoursToCoach = Configuration["INAPPNotificationTemplateConfig:GroupCoachingCancellationLessthan24HoursToCoach"],
                    GroupCoachingCancellationMorethan24HoursToCoach = Configuration["INAPPNotificationTemplateConfig:GroupCoachingCancellationMorethan24HoursToCoach"],
                    ShareEventToPlayer = Configuration["INAPPNotificationTemplateConfig:ShareEventToPlayer"],
                    CaptainAcceptsTheRequestToPlayer = Configuration["INAPPNotificationTemplateConfig:CaptainAcceptsTheRequestToPlayer"],
                    OneSpotIsFreeFromWaitingListToPlayer = Configuration["INAPPNotificationTemplateConfig:OneSpotIsFreeFromWaitingListToPlayer"],
                    PitchBookingConfirmationToFacility = Configuration["INAPPNotificationTemplateConfig:PitchBookingConfirmationToFacility"],
                    PitchBookingCancellationFromCaptainMorethan24HoursToFacility = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromCaptainMorethan24HoursToFacility"],
                    PitchBookingCancellationFromPlayerMorethan24HoursToFacility = Configuration["INAPPNotificationTemplateConfig:PitchBookingCancellationFromPlayerMorethan24HoursToFacility"],
                },
                AppleSignInConfig = new AppleSignInConfiguration
                {
                    ClientId = Configuration["AppleSignIn:ClientId"],
                    TeamId = Configuration["AppleSignIn:TeamId"],
                    KeyId = Configuration["AppleSignIn:KeyId"],
                    PrivateKey = Configuration["AppleSignIn:PrivateKey"],
                    ValidationUrl = Configuration["AppleSignIn:ValidationUrl"],
                    ValidIssuerUrl = Configuration["AppleSignIn:ValidIssuerUrl"],
                    PublicKeysUrl = Configuration["AppleSignIn:PublicKeysUrl"]
                }
            };
            masterConfig.MapUrl = Configuration["Link:MapUrl"];
            masterConfig.HostURL = Configuration["Link:BaseUrl"];
            masterConfig.DefaultClientSite = Configuration["Link:StaticSite"];
            services.AddSingleton(masterConfig);
            #endregion "ConfigurationBuilder"

            #region "JWT TOKEN Init"
            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Tokens:Key"]));
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = Configuration["Link:Issuer"];
                options.Audience = Configuration["Link:Audience"];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });


            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Configuration["Link:Issuer"],

                ValidateAudience = true,
                ValidAudience = Configuration["Link:Audience"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = Configuration["Link:Issuer"];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
            });
            #endregion

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sidekick API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                     {
                           new OpenApiSecurityScheme
                             {
                                 Reference = new OpenApiReference
                                 {
                                     Type = ReferenceType.SecurityScheme,
                                     Id = "Bearer"
                                 }
                             },
                             new string[] {}
                     }
                 });
            });

            services.ConfigureLoggerService();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddMvc().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, APIDBContext db)
        {

            bool enablieDBRowsing = false;
            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sidekick API V1");
                });
                enablieDBRowsing = true;
            }

            db.Database.EnsureCreated();

            app.UseAuthentication();

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Resources")),
                RequestPath = new PathString("/Resources"),
                EnableDirectoryBrowsing = enablieDBRowsing
            });

            app.UseMvc();

        }
    }
}
