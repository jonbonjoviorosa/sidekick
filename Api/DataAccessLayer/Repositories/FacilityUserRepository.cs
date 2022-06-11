
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class FacilityUserRepository : APIBaseRepo, IFacilityUserRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public FacilityUserRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse LoginFacilityUser(LoginCredentials _user)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityUserRepository::LoginFacilityUser --");
            LogManager.LogDebugObject(_user);

            try
            {
                FacilityUser facilityUser = DbContext.FacilityUsers.Where(a => a.Email == _user.Email).FirstOrDefault();
                if (facilityUser != null)
                {
                    if (facilityUser.IsEnabled == false)
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Login failed. Inactive Account.",
                            Status = "Failed!",
                            StatusCode = System.Net.HttpStatusCode.Unauthorized
                        };
                    }

                    var hashPass = facilityUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key);
                    if (facilityUser.Password == facilityUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key))
                    {
                        if (facilityUser.IsLocked == true)
                        {
                            return apiResp = new APIResponse
                            {
                                Message = "You need to change first your password to activate your account.",
                                Status = "Success!",
                                StatusCode = System.Net.HttpStatusCode.Redirect,
                                ResponseCode = APIResponseCode.IsFacilityUser,
                                Payload = new FacilityUserContext
                                {
                                    FacilityUserInfo = facilityUser,
                                    Tokens = AddFacilityUserLoginTransaction(facilityUser)
                                }
                            };
                        }
                        else
                        {
                            return apiResp = new APIResponse
                            {
                                Message = "Login succesful.",
                                Status = "Success!",
                                StatusCode = System.Net.HttpStatusCode.OK,
                                ResponseCode = APIResponseCode.IsFacilityUser,
                                Payload = new FacilityUserContext
                                {
                                    FacilityUserInfo = facilityUser,
                                    Tokens = AddFacilityUserLoginTransaction(facilityUser)
                                }
                            };
                        }
                    }
                    else
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Login Failed. Incorrect Password.",
                            Status = "Failed!",
                            StatusCode = System.Net.HttpStatusCode.Unauthorized
                        };
                    }
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Login Failed. Email not found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }
            }
            catch (SqlException ex)
            {
                LogManager.LogInfo("-- Error::FaciityUserRepository::LoginFacilityUser --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public FacilityUserLoginTransaction AddFacilityUserLoginTransaction(FacilityUser _user)
        {
            LogManager.LogInfo("-- Run::FaciityUserRepository::AddFacilityUserLoginTransaction --");
            LogManager.LogDebugObject(_user);

            DateTime ExpirationDates = DateTime.Now;
            FacilityUserLoginTransaction existingLoginTrans = new FacilityUserLoginTransaction();
            FacilityUserLoginTransaction newLoginTrans = new FacilityUserLoginTransaction();

            try
            {
                existingLoginTrans = DbContext.FacilityUserLoginTransactions
                               .Where(u => u.FacilityUserId == _user.FacilityUserId &&
                                           u.DevicePlatform == EDevicePlatform.Web &&
                                           u.IsEnabled == true &&
                                           u.TokenExpiration > DateTime.Now)
                               .OrderByDescending(u => u.CreatedDate)
                               .FirstOrDefault();

                if (existingLoginTrans != null) 
                {
                    existingLoginTrans.IsEnabled = false;

                    DbContext.FacilityUserLoginTransactions.Update(existingLoginTrans);
                    DbContext.SaveChanges();
                }

                newLoginTrans = new FacilityUserLoginTransaction
                {
                    FacilityUserId = _user.FacilityUserId,
                    DevicePlatform = EDevicePlatform.Web,
                    Token = CreateJWTTokenForFacilityAdmin(_user, ExpirationDates, APIConfig),
                    TokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp),
                    RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                    RefreshTokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp * 2),
                    CreatedDate = DateTime.Now,
                    CreatedBy = _user.FacilityUserId,
                    IsEnabled = true,
                    IsLocked = false
                };

                DbContext.FacilityUserLoginTransactions.Add(newLoginTrans);
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityUserRepository::AddFacilityUserLoginTransaction --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
            }

            return newLoginTrans;
        }

        public APIResponse AddFacilityUser(string _auth, FacilityUserProfile _user)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityUserRepository::AddFacilityUser --");
            LogManager.LogDebugObject(_user);

            try
            {
                FacilityUserLoginTransaction IsUserLoggedIn = DbContext.FacilityUserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                FacilityUser facilityUser = DbContext.FacilityUsers.Where(u => u.Email == _user.Email).FirstOrDefault();
                if (facilityUser == null)
                {
                    Guid GuidId = Guid.NewGuid();
                    DateTime RegistrationDate = DateTime.Now;
                    var facilityRoleName = DbContext.FacilityUserTypes.Where(f => f.FacilityRoleId == _user.FacilityRoleId).FirstOrDefault();

                    FacilityUser newUser = new FacilityUser
                    {
                        FacilityId = _user.FacilityId,
                        FacilityUserId = GuidId,
                        //FacilityUserType = _user.facility,
                        FacilityRoleId = _user.FacilityRoleId,
                        FacilityAccountType = EAccountType.FACILITYUSER,
                        FacilityRole = facilityRoleName != null ? facilityRoleName.FacilityRoleName : string.Empty,

                        Email = _user.Email,
                        FirstName = _user.FirstName,
                        LastName = _user.LastName,
                        MobileNumber = _user.MobileNumber,
                        ImageUrl = string.IsNullOrWhiteSpace(_user.ImageUrl) ? APIConfig.HostURL + "/resources/Defaults/User_Default_Logo.jpg" : _user.ImageUrl,
                        DevicePlatform = EDevicePlatform.Web,

                        LastEditedBy = IsUserLoggedIn.FacilityUserId,
                        LastEditedDate = RegistrationDate,
                        CreatedBy = IsUserLoggedIn.FacilityUserId,
                        CreatedDate = RegistrationDate,
                        IsEnabled = true,
                        IsEnabledBy = IsUserLoggedIn.FacilityUserId,
                        DateEnabled = RegistrationDate,
                        IsLocked = true,
                        LockedDateTime = RegistrationDate
                    };
                    newUser.Password = newUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key);

                    DbContext.FacilityUsers.Add(newUser);
                    DbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "New record added successfully.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Processing Failed. Duplicate email found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityUserRepository::AddFacilityUser --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse EditFacilityUser(string _auth, FacilityUserProfile _user)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityUserRepository::EditFacilityUser --");
            LogManager.LogDebugObject(_user);

            try
            {
                FacilityUserLoginTransaction IsUserLoggedIn = DbContext.FacilityUserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                FacilityUser facilityUser = DbContext.FacilityUsers.Where(u => u.FacilityUserId == _user.FacilityUserId).FirstOrDefault();
                if (facilityUser != null)
                {
                    FacilityUser hasDuplicate = DbContext.FacilityUsers.Where(u => u.Email == _user.Email).FirstOrDefault();
                    if (hasDuplicate != null && hasDuplicate.FacilityUserId == facilityUser.FacilityUserId)
                    {
                        DateTime DateUpdated = DateTime.Now;

                        facilityUser.LastEditedBy = IsUserLoggedIn.FacilityUserId;
                        facilityUser.LastEditedDate = DateUpdated;
                        facilityUser.IsEnabled = true;
                        facilityUser.IsEnabledBy = IsUserLoggedIn.FacilityUserId;
                        facilityUser.DateEnabled = DateUpdated;

                        facilityUser.Email = _user.Email;
                        facilityUser.FirstName = _user.FirstName;
                        facilityUser.LastName = _user.LastName;
                        facilityUser.MobileNumber = _user.MobileNumber;
                        facilityUser.ImageUrl = string.IsNullOrWhiteSpace(_user.ImageUrl) ? "https://api.sidekick.fit/resources/Defaults/Facility_Default_Logo.jpg" : _user.ImageUrl;
                        //facilityUser.FacilityUserType = _user.FacilityUserType;
                        if(_user.FacilityUserType != EFacilityUserType.OWNER)
                        {
                            var facilityRoleName = DbContext.FacilityUserTypes.Where(f => f.FacilityRoleId == _user.FacilityRoleId).FirstOrDefault();
                            facilityUser.FacilityRoleId = _user.FacilityRoleId;
                            facilityUser.FacilityRole = facilityRoleName != null ? facilityRoleName.FacilityRoleName : string.Empty;
                        }
                       
                        facilityUser.DevicePlatform = _user.DevicePlatform;
                        if (!string.IsNullOrWhiteSpace(_user.Password))
                        {
                            facilityUser.Password = facilityUser.HashP(_user.Password, APIConfig.TokenKeys.Key);
                        }

                        DbContext.FacilityUsers.Update(facilityUser);
                        DbContext.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "Record updated successfully.",
                            Status = "Success!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Processing Failed. Duplicate email found.",
                            Status = "Failed!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Processing Failed. Record not found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (SqlException ex)
            {
                LogManager.LogInfo("-- Error::FaciityUserRepository::EditFacilityUser --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse GetFacilityUser(Guid _guid, bool _takeAll)
        {
            APIResponse aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityUserRepository::GetFacilityUser --");
            LogManager.LogDebugObject(_guid);

            try
            {
                if (_takeAll)
                {
                    IEnumerable<FacilityUserList> facilityUsers = null;
                    facilityUsers = DbContext.FacilityUsers.AsNoTracking()
                        .Where(w => w.FacilityId == _guid)
                        .Select(i => new FacilityUserList
                        {
                            FacilityUserId = i.FacilityUserId,
                            FullName = i.FirstName + " " + i.LastName,
                            ImageUrl = string.IsNullOrWhiteSpace(i.ImageUrl) ? "https://api.sidekick.fit/resources/Defaults/Facility_Default_Logo.jpg" : i.ImageUrl,
                            Email = i.Email,
                            FacilityUserType = i.FacilityUserType,
                            CreatedDate = i.CreatedDate,
                            IsEnabled = i.IsEnabled
                        }).ToList();

                    return aResp = new APIResponse
                    {
                        Message = "All records found.",
                        Status = "Success!",
                        Payload = facilityUsers,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return aResp = new APIResponse
                    {
                        Message = "Record found.",
                        Status = "Success!",
                        Payload = DbContext.FacilityUsers.AsNoTracking()
                                            .Where(u => u.FacilityUserId == _guid)
                                            .Select(i => new FacilityUserProfile
                                            {
                                                FacilityUserId = i.FacilityUserId,
                                                FacilityId = i.FacilityId,
                                                Email = i.Email,
                                                Password = i.Password,
                                                FirstName = i.FirstName,
                                                LastName = i.LastName,
                                                MobileNumber = i.MobileNumber,
                                                ImageUrl = string.IsNullOrWhiteSpace(i.ImageUrl) ? "https://api.sidekick.fit/resources/Defaults/Facility_Default_Logo.jpg" : i.ImageUrl,
                                                FacilityUserType = i.FacilityUserType,
                                                FacilityRoleId = i.FacilityRoleId,
                                                IsEnabled = i.IsEnabled
                                            }).FirstOrDefault(),
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityUserRepository::GetFacilityUser --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse FacilityUserChangePassword(string _auth, FacilityUserChangePassword _user)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityUserRepository::FacilityUserChangePassword --");
            LogManager.LogDebugObject(_user);

            try
            {
                FacilityUserLoginTransaction IsUserLoggedIn = DbContext.FacilityUserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                FacilityUser facilityUser = DbContext.FacilityUsers.Where(u => u.FacilityUserId == _user.FacilityUserId).FirstOrDefault();

                string HashedCurrPass = facilityUser.HashP(_user.CurrentPassword.ToString(), APIConfig.TokenKeys.Key);
                string HashedNewPass = facilityUser.HashP(_user.NewPassword.ToString(), APIConfig.TokenKeys.Key);

                if (facilityUser.Password == HashedCurrPass)
                {
                    DateTime DateUpdated = DateTime.Now;

                    facilityUser.LastEditedBy = IsUserLoggedIn.FacilityUserId;
                    facilityUser.LastEditedDate = DateUpdated;

                    if (facilityUser.IsLocked == true)
                    {
                        facilityUser.IsLocked = false;
                        facilityUser.LockedDateTime = DateUpdated;
                    }

                    facilityUser.Password = HashedNewPass;

                    DbContext.FacilityUsers.Update(facilityUser);
                    DbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Password successfully changed.",
                        Status = "Success",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Incorrect current password.",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityUserRepository::FacilityUserChangePassword --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse FacilityUserForgotPassword(FacilityUserForgotPassword _user, APIConfigurationManager _conf = null)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityUserRepository::FacilityUserForgotPassword --");
            LogManager.LogDebugObject(_user);

            try
            {
                FacilityUser facilityUser = DbContext.FacilityUsers.Where(u => u.Email == _user.Email).FirstOrDefault();
                if (facilityUser != null)
                {
                    if (facilityUser.IsEnabled == false)
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Your account is still not active. Contact support to activate your account.",
                            Status = "Failed",
                            StatusCode = System.Net.HttpStatusCode.Unauthorized
                        };
                    }
                    else
                    {
                        var sysGenPassword = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
                        var emailBody = String.Format(APIConfig.MsgConfigs.ForgotPassword, facilityUser.FirstName.ToUpper(), sysGenPassword);

                        var EmailParam = _conf.MailConfig;
                        EmailParam.To = new List<string>() { facilityUser.Email };
                        EmailParam.Subject = "Sidekick: Facility User Forgot Password";
                        EmailParam.Body = emailBody;

                        EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(_conf, EmailParam.Body, LogManager, false);

                        var sendStatus = SendEmailByEmailAddress(new List<string>() { facilityUser.Email }, EmailParam, LogManager);
                        if (sendStatus == 0)
                        {
                            DateTime DateUpdated = DateTime.Now;

                            facilityUser.Password = facilityUser.HashP(sysGenPassword.ToString(), APIConfig.TokenKeys.Key);
                            facilityUser.LastEditedBy = facilityUser.FacilityUserId;
                            facilityUser.LastEditedDate = DateUpdated;

                            DbContext.FacilityUsers.Update(facilityUser);
                            DbContext.SaveChanges();

                            return apiResp = new APIResponse
                            {
                                Message = "Your new password is sent successfully to your email: " + facilityUser.Email,
                                Status = "Success",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            return apiResp = new APIResponse
                            {
                                Message = "Sending Failed. Please try again.",
                                Status = "Failed",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Record not found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityUserRepository::FacilityUserForgotPassword --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse FacilityUserChangeStatus(string _auth, ChangeStatus _user)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityUserRepository::FacilityUserChangeStatus --");
            LogManager.LogDebugObject(_user);

            try
            {
                FacilityUserLoginTransaction IsUserLoggedIn = DbContext.FacilityUserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                FacilityUser facilityUser = DbContext.FacilityUsers.Where(a => a.FacilityUserId == _user.GuID).FirstOrDefault();
                if (facilityUser != null)
                {
                    DateTime DateUpdated = DateTime.Now;

                    facilityUser.LastEditedBy = IsUserLoggedIn.FacilityUserId;
                    facilityUser.LastEditedDate = DateUpdated;
                    facilityUser.IsEnabled = _user.IsEnabled;
                    facilityUser.IsEnabledBy = IsUserLoggedIn.FacilityUserId;
                    facilityUser.DateEnabled = DateUpdated;

                    DbContext.FacilityUsers.Update(facilityUser);
                    DbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Record updated successfully.",
                        Status = "Success",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Processing Failed. Record not found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityUserRepository::FacilityUserChangeStatus --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> GetUserRole(Guid facilityRoleId)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityUserRepository::GetUserRole --");
            LogManager.LogDebugObject(facilityRoleId);
            try
            {
                var userRole = await DbContext.FacilityUserTypes.Where(f => f.FacilityRoleId == facilityRoleId).FirstOrDefaultAsync();
                if(userRole != null)
                {
                    return new APIResponse
                    {
                        Payload = userRole.FacilityRoleName,
                        Message = "Retrieved role for Facility User",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                return new APIResponse
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityUserRepository::GetUserRole --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }
    }
}
