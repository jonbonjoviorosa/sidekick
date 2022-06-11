
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class SocialLoginRepository : APIBaseRepo, ISocialLoginRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        private IMainHttpClient MainHttpClient { get; }

        ILoggerManager LogManager { get; }
        IUserRepository IUserRepo { get; }
        ICoachRepository ICoachRepo { get; }

        public SocialLoginRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IMainHttpClient _mhttpc, IUserRepository _uRepo, ICoachRepository _iCoachRepo)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            MainHttpClient = _mhttpc;
            IUserRepo = _uRepo;
            ICoachRepo = _iCoachRepo;
        }

        public APIResponse AppleSignIn(LoginAppleCredentials _user)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::AppleSignIn --");
            LogManager.LogDebugObject(_user);

            try
            {
                string clientSecretKey = GetClientSecret();
                if (string.IsNullOrEmpty(clientSecretKey))
                {
                    LogManager.LogError("No client secret generated.");
                    return apiResp = new APIResponse
                    {
                        Message = "No client secret generated.",
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                //string clientSecretKey = System.IO.File.ReadAllText("Service/ServiceKeys/AppleClientSecret.p8");
                AppleTokenDto getAppleToken = GenerateAppleAuthCodeToken(clientSecretKey, _user.AuthCode);
                if (!string.IsNullOrEmpty(getAppleToken.error))
                {
                    LogManager.LogError("Authorization Grant Code Error : " + getAppleToken.error + " : " + getAppleToken.error_description);
                    return apiResp = new APIResponse
                    {
                        Message = "Authorization Grant Code Error : " + getAppleToken.error + " : " + getAppleToken.error_description,
                        Status = Status.Failed,
                        Payload = getAppleToken,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                string validateAuthGrantCode = ValidateAppleIDToken(getAppleToken.id_token);
                if (string.IsNullOrEmpty(validateAuthGrantCode))
                {
                    LogManager.LogError("Unauthorized/Invalid Authorization Grant ID Token.");
                    return apiResp = new APIResponse
                    {
                        Message = "Unauthorized/Invalid Authorization Grant Token.",
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                UserContext signInUser = SignInUser(_user, validateAuthGrantCode);
                if (signInUser != null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Login Successful.",
                        Status = Status.Success,
                        Payload = new
                        {
                            signInUser.UserInfo,
                            signInUser.CoachProfile,
                            signInUser.Tokens,
                            signInUser.UnReadCount,
                            RefreshToken = getAppleToken.refresh_token
                        },
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Login unsuccessful.",
                    Status = Status.Failed,
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::AppleSignIn --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse ValidateAppleToken(string _refreshToken)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::ValidateAppleToken -- ");

            try
            {
                string clientSecretKey = GetClientSecret();
                if (string.IsNullOrEmpty(clientSecretKey))
                {
                    LogManager.LogError("No client secret generated.");
                    return apiResp = new APIResponse
                    {
                        Message = "No client secret generated.",
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                AppleTokenDto validateRefreshToken = GenerateAppleRefreshCodeToken(clientSecretKey, _refreshToken);
                if (!string.IsNullOrEmpty(validateRefreshToken.error))
                {
                    LogManager.LogError("Refresh Token Code Error >> " + validateRefreshToken.error + " : " + validateRefreshToken.error_description);
                    return apiResp = new APIResponse
                    {
                        Message = "Refresh Token Code Error : " + validateRefreshToken.error + " : " + validateRefreshToken.error_description,
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                string validateAuthGrantCode = ValidateAppleIDToken(validateRefreshToken.id_token);
                if (string.IsNullOrEmpty(validateAuthGrantCode))
                {
                    LogManager.LogError("Unauthorized/Invalid Authorization Refresh ID Token.");
                    return apiResp = new APIResponse
                    {
                        Message = "Unauthorized/Invalid Authorization Refresh Token.",
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = Status.Success,
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::ValidateAppleToken --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse GetAppleClientSecret()
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::GenerateAppleClientSecret -- ");

            try
            {
                string getClientSecret = GenerateAppleClientSecret();
                if (string.IsNullOrEmpty(getClientSecret))
                {
                    LogManager.LogError("No client secret generated.");
                    return apiResp = new APIResponse
                    {
                        Message = "No client secret generated.",
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = Status.Success,
                    Status = Status.Success,
                    Payload = getClientSecret,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::GenerateAppleClientSecret --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        #region Helpers

        public string GetClientSecret()
        {
            return DbContext.ExternalConfigKeys.Where(x => x.IsEnabled == true).FirstOrDefault().Key;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public string GenerateAppleClientSecret()
        {
            LogManager.LogInfo("-- Run::GenerateAppleClientSecret -- ");

            //string privateKey = System.IO.File.ReadAllText("Service/ServiceKeys/AppleAuthKey.p8");
            string privateKey = APIConfig.AppleSignInConfig.PrivateKey;
            var cngKey = CngKey.Import(Convert.FromBase64String(privateKey), CngKeyBlobFormat.Pkcs8PrivateBlob);

            var securityKey = new ECDsaSecurityKey(new ECDsaCng(cngKey))
            {
                KeyId = APIConfig.AppleSignInConfig.KeyId
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = APIConfig.AppleSignInConfig.TeamId,
                Audience = APIConfig.AppleSignInConfig.ValidIssuerUrl,
                Subject = new ClaimsIdentity(new[] { new Claim("sub", APIConfig.AppleSignInConfig.ClientId) }),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256)
            };

            var handler = new JwtSecurityTokenHandler();

            var token = handler.CreateJwtSecurityToken(descriptor);

            return handler.WriteToken(token);
        }

        AppleTokenDto GenerateAppleAuthCodeToken(string _clientSecret, string _authorizationCode)
        {
            AppleTokenRequestDto appleParams = new();
            string appleValidationUrl = APIConfig.AppleSignInConfig.ValidationUrl + GenerateAppleAuthCodeParams(_clientSecret, _authorizationCode);
            var response = MainHttpClient.PostAppleHttpClientRequest(appleValidationUrl, appleParams);
            return JsonConvert.DeserializeObject<AppleTokenDto>(response);
        }

        string GenerateAppleAuthCodeParams(string clientSecret, string authorizationCode)
        {
            var requestBody = new List<KeyValuePair<string, string>>()
            {
               new KeyValuePair<string, string>(AppleSignInConst.client_id, APIConfig.AppleSignInConfig.ClientId),
                        new KeyValuePair<string, string>(AppleSignInConst.client_secret, clientSecret),
                        new KeyValuePair<string, string>(AppleSignInConst.grant_type, AppleSignInConst.authorization_code),
                        new KeyValuePair<string, string>(AppleSignInConst.code_type, authorizationCode)
            };

            var content = new FormUrlEncodedContent(requestBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var Res = new HttpRequestMessage(HttpMethod.Post, "https://appleid.apple.com/auth/token") { Content = content };
            return Res.Content.ReadAsStringAsync().Result;
        }

        string ValidateAppleIDToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
                var claims = jwtSecurityToken.Claims;

                var expirationClaim = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value;
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expirationClaim)).DateTime;

                if (expirationTime < DateTime.Now) { return null; }

                if (claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud).Value != APIConfig.AppleSignInConfig.ClientId) { return null; }

                if (claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Iss).Value != APIConfig.AppleSignInConfig.ValidIssuerUrl) { return null; }

                return claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::ValidateAppleIDToken --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                return null;
            }
        }

        AppleTokenDto GenerateAppleRefreshCodeToken(string _clientSecret, string _refreshToken)
        {
            AppleTokenRequestDto appleParams = new();
            string appleValidationUrl = APIConfig.AppleSignInConfig.ValidationUrl + GenerateAppleRefreshCodeParams(_clientSecret, _refreshToken);
            var response = MainHttpClient.PostAppleHttpClientRequest(appleValidationUrl, appleParams);
            return JsonConvert.DeserializeObject<AppleTokenDto>(response);
        }

        string GenerateAppleRefreshCodeParams(string clientSecret, string refreshToken)
        {
            var requestBody = new List<KeyValuePair<string, string>>()
            {
               new KeyValuePair<string, string>(AppleSignInConst.client_id, APIConfig.AppleSignInConfig.ClientId),
                        new KeyValuePair<string, string>(AppleSignInConst.client_secret, clientSecret),
                        new KeyValuePair<string, string>(AppleSignInConst.grant_type, AppleSignInConst.refresh_token),
                        new KeyValuePair<string, string>(AppleSignInConst.refresh_token, refreshToken)
            };

            var content = new FormUrlEncodedContent(requestBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var Res = new HttpRequestMessage(HttpMethod.Post, "https://appleid.apple.com/auth/token") { Content = content };
            return Res.Content.ReadAsStringAsync().Result;
        }

        UserContext SignInUser(LoginAppleCredentials _user, string _email)
        {
            UserContext userContext = new();

            User foundUser = DbContext.Users.Where(u => u.Email == _email && u.AppleUserId == _user.AppleId).FirstOrDefault();
            if (foundUser == null)
            {
                Guid UserGuid = Guid.NewGuid();
                DateTime RegistrationDate = DateTime.Now;
                User NewUser = new()
                {
                    UserId = UserGuid,
                    Email = _email,
                    FirstName = _user.FirstName,
                    LastName = _user.LastName,
                    Gender = Genders.Others,
                    UserType = EUserType.NORMAL,
                    MobileNumber = null,
                    DateOfBirth = null,
                    DeviceRegistrationPlatform = EDevicePlatform.IOS,
                    UserRegistrationPlatform = UserRegistrationPlatform.Apple,
                    LastEditedBy = UserGuid,
                    LastEditedDate = RegistrationDate,
                    CreatedBy = UserGuid,
                    CreatedDate = RegistrationDate,
                    IsEnabledBy = UserGuid,
                    IsEnabled = true,
                    DateEnabled = RegistrationDate,
                    IsLocked = false,
                    LockedDateTime = null,
                };

                return userContext = new UserContext
                {
                    UserInfo = NewUser,
                    Tokens = IUserRepo.AddLoginTransactionForUser(NewUser)
                };
            }
            else
            {
                CoachProfile UserCoachProfile = new();
                if (foundUser.UserType == EUserType.NORMALANDCOACH)
                {
                    UserCoachProfile = ICoachRepo.GetCoachProfile(foundUser.UserId);
                }

                return userContext = new UserContext
                {
                    UserInfo = foundUser,
                    CoachProfile = UserCoachProfile,
                    Tokens = IUserRepo.AddLoginTransactionForUser(foundUser),
                    UnReadCount = DbContext.ChatConversations.Where(c => c.UserId == foundUser.UserId && c.IsUnRead == false).Count()
                };
            }
        }
    }

    #endregion
}

