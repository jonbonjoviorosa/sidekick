
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Api.Configurations.Resources;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class AdminRepository : APIBaseRepo, IAdminRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        IFacilityUserRepository IFacilityUserRepo;

        public AdminRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IFacilityUserRepository _iFCUserRepo)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            IFacilityUserRepo = _iFCUserRepo;
        }

        public APIResponse RegisterAdmin(AdminProfile _admin)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::AdminRepository::RegisterAdmin --");
            LogManager.LogDebugObject(_admin);

            try
            {
                Admin FoundUser = DbContext.Admins.Where(u => u.Email == _admin.Email).FirstOrDefault();
                if (FoundUser == null)
                {
                    Guid AdminGuidID = Guid.NewGuid();
                    DateTime RegistrationDate = DateTime.Now;

                    Admin NewUser = new Admin
                    {
                        Email = _admin.Email,
                        FirstName = _admin.FirstName,
                        LastName = _admin.LastName,
                        AdminType = _admin.AdminType,

                        LastEditedBy = _admin.AdminId,
                        LastEditedDate = RegistrationDate,
                        CreatedBy = _admin.AdminId,
                        CreatedDate = RegistrationDate,
                        IsEnabledBy = _admin.AdminId,
                        IsEnabled = true,
                        DateEnabled = RegistrationDate,
                        IsLocked = false,
                        LockedDateTime = null
                    };
                    NewUser.Password = NewUser.HashP(_admin.Password.ToString(), APIConfig.TokenKeys.Key);

                    DbContext.Add(NewUser);
                    DbContext.SaveChanges();

                    apiResp.Message = "New user added successfully.";
                    apiResp.Status = "Success!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    apiResp.Payload = NewUser;
                }
                else
                {
                    LogManager.LogWarn("Email Address:" + FoundUser.Email + " was already in use!");
                    FoundUser.Password = "";
                    LogManager.LogDebugObject(FoundUser);

                    apiResp = new APIResponse
                    {
                        Message = "The email address is already used.",
                        Status = "Error",
                        StatusCode = System.Net.HttpStatusCode.Found
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::AdminRepository::RegisterAdmin --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = Status.InternalServerError;
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse AdminLogin(LoginCredentials _admin)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::AdminRepository::AdminLogin --");
            LogManager.LogDebugObject(_admin);

            try
            {
                FacilityUser facilityUser = DbContext.FacilityUsers.AsNoTracking().Where(u => u.Email == _admin.Email).FirstOrDefault();
                if (facilityUser != null && facilityUser.FacilityAccountType == EAccountType.FACILITYUSER)
                {
                    return IFacilityUserRepo.LoginFacilityUser(_admin);
                }

                Admin foundAdmin = DbContext.Admins.Where(a => a.Email == _admin.Email).FirstOrDefault();
                if (foundAdmin != null)
                {
                    if (foundAdmin.IsEnabled == false)
                    {
                        return apiResp = new APIResponse
                        {
                            Message = Messages.LoginFailed,
                            Status = Status.Failed,
                            StatusCode = System.Net.HttpStatusCode.Unauthorized
                        };
                    }

                    if (foundAdmin.Password == foundAdmin.HashP(_admin.Password.ToString(), APIConfig.TokenKeys.Key))
                    {
                        if (foundAdmin.IsLocked == true)
                        {
                            return apiResp = new APIResponse
                            {
                                Message = "You need to change first your password to activate your account.",
                                Status = "Success!",
                                StatusCode = System.Net.HttpStatusCode.Redirect,
                                ResponseCode = APIResponseCode.IsSuperAdminUser,
                                Payload = new AdminUserContext
                                {
                                    AdminInfo = foundAdmin,
                                    Tokens = AddLoginTransactionForAdminUser(foundAdmin)
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
                                ResponseCode = APIResponseCode.IsSuperAdminUser,
                                Payload = new AdminUserContext
                                {
                                    AdminInfo = foundAdmin,
                                    Tokens = AddLoginTransactionForAdminUser(foundAdmin)
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
                LogManager.LogInfo("-- Error::AdminRepository::AdminLogin --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public AdminLoginTransaction AddLoginTransactionForAdminUser(Admin _admin)
        {
            DateTime ExpirationDates = DateTime.Now;
            AdminLoginTransaction ulT = new AdminLoginTransaction();
            //
            // check if there is an existing login transaction that is not yet expired 
            // for the same platform
            //
            try
            {
                ulT = DbContext.AdminLoginTransactions
                               .Where(u => u.AdminId == _admin.AdminId &&
                                           u.Device == EDevicePlatform.Web &&
                                           u.IsEnabled == true &&
                                           u.TokenExpiration > DateTime.Now)
                               .OrderByDescending(u => u.DateCreated)
                               .FirstOrDefault();
                if (ulT == null)
                {
                    //
                    // there was no active login transaction
                    //
                    ulT = new AdminLoginTransaction
                    {
                        AdminId = _admin.AdminId,
                        Device = EDevicePlatform.Web,
                        AdminType = _admin.AdminType,
                        Token = CreateJWTTokenForAdmin(_admin, ExpirationDates, APIConfig),
                        TokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp),
                        RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                        RefreshTokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp * 2),
                        CreatedDate = DateTime.Now,
                        CreatedBy = _admin.AdminId,
                        IsEnabled = true,
                        IsLocked = false
                    };

                    // Invalidate existing active admin login transactions (cleanup)
                    var InvalidTransactions = DbContext.AdminLoginTransactions
                                                .Where(u => u.AdminId == _admin.AdminId &&
                                                            u.Device == EDevicePlatform.Web &&
                                                            u.IsEnabled == true);
                    foreach (var InvalidTrans in InvalidTransactions)
                    {
                        InvalidTrans.IsEnabled = false;
                        DbContext.AdminLoginTransactions.Update(InvalidTrans);
                    }

                    DbContext.Add(ulT);
                    DbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::AdminRepository::AddLoginTransactionForAdminUser --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
            }

            return ulT;
        }

        public APIResponse ReGenerateTokens(AdminLoginTransaction _loginParam)
        {
            APIResponse ApiResp = new APIResponse();

            LogManager.LogInfo("-- Run::AdminRepository::ReGenerateTokens --");
            LogManager.LogDebugObject(_loginParam);

            try
            {
                LogManager.LogDebugObject(_loginParam);

                Admin foundUser = DbContext.Admins.Where(u => u.AdminId == _loginParam.AdminId).FirstOrDefault();
                DateTime ExpirationDates = DateTime.Now;

                AdminLoginTransaction returnUser = new AdminLoginTransaction();

                if (foundUser != null)
                {
                    // Get latest login transaction by date to get latest user token
                    var FoundTransaction = DbContext.AdminLoginTransactions.Where(u => u.AdminId == _loginParam.AdminId && u.IsEnabled == true).OrderByDescending(u => u.DateCreated).FirstOrDefault();

                    if (FoundTransaction != null && ExpirationDates < FoundTransaction.RefreshTokenExpiration)   //found user && RT not yet expired)
                    {

                        FoundTransaction.IsEnabled = false;
                        DbContext.Update(FoundTransaction);

                        returnUser = new AdminLoginTransaction
                        {
                            AdminId = FoundTransaction.AdminId,
                            AdminType = FoundTransaction.AdminType,
                            Token = CreateJWTTokenForAdmin(foundUser, ExpirationDates, APIConfig),
                            TokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp),
                            RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                            RefreshTokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp * 2),
                            Device = FoundTransaction.Device,
                            IsEnabled = true,
                            DateCreated = ExpirationDates
                        };

                        ApiResp.Message = "Success";
                        ApiResp.Status = "Activated!";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                        DbContext.Add(returnUser);
                        DbContext.SaveChanges();
                    }
                    else
                    {
                        // Deactivate user login transaction
                        FoundTransaction.IsEnabled = false;

                        ApiResp.Message = "Success";
                        ApiResp.Status = "Deactivated!";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                        DbContext.Update(FoundTransaction);
                        DbContext.SaveChanges();

                        returnUser = FoundTransaction;
                    }
                }

                ApiResp.Payload = new AdminUserContext
                {
                    AdminInfo = foundUser,
                    Tokens = returnUser
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::AdminRepository::ReGenerateTokens --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                ApiResp.Message = "Something Went wrong!";
                ApiResp.Status = "Internal Server Error";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }

            return ApiResp;
        }

        public APIResponse GetAllAdmins(int _admin)
        {
            APIResponse aResp = new APIResponse();

            LogManager.LogInfo("-- Run::AdminRepository::ReGeneGetAllAdminsrateTokens --");
            LogManager.LogDebugObject(_admin);

            try
            {
                if (_admin == 0)
                {
                    IEnumerable<AdminList> admins = null;
                    admins = DbContext.Admins.AsNoTracking()
                        .Select(i => new AdminList
                        {
                            AdminId = i.AdminId,
                            FullName = i.FirstName + " " + i.LastName,
                            AdminType = i.AdminType,
                            IsActive = i.IsEnabled
                        });

                    if (admins.Count() > 0)
                    {
                        aResp = new APIResponse
                        {
                            Message = "Fetched successfully.",
                            Status = "Success!",
                            Payload = admins,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Records found.",
                        Status = "Success!",
                        Payload = DbContext.Admins.AsNoTracking().FirstOrDefault(u => u.Id == _admin),
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::AdminRepository::GetAllAdmins --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse EditAdminProfile(AdminProfile _admin)
        {
            APIResponse ApiResp = new APIResponse();

            LogManager.LogInfo("-- Run::AdminRepository::EditAdminProfile --");
            LogManager.LogDebugObject(_admin);

            try
            {
                //Check if user exist
                var foundAdmin = DbContext.Admins.FirstOrDefault(u => u.AdminId == _admin.AdminId);
                if (foundAdmin != null)
                {
                    foundAdmin.LastEditedDate = DateTime.Now;
                    foundAdmin.LastEditedBy = _admin.AdminId;
                    foundAdmin.DateEnabled = DateTime.Now;
                    foundAdmin.IsEnabled = _admin.IsEnabled;
                    foundAdmin.IsEnabledBy = _admin.AdminId;

                    foundAdmin.FirstName = _admin.FirstName;
                    foundAdmin.LastName = _admin.LastName;
                    foundAdmin.Password = foundAdmin.HashP(_admin.Password.ToString(), APIConfig.TokenKeys.Key);
                    foundAdmin.AdminType = _admin.AdminType;

                    DbContext.Admins.Update(foundAdmin);
                    DbContext.SaveChanges();

                    ApiResp.Message = "User profile has been updated successfully.";
                    ApiResp.Status = "Success";
                    ApiResp.Payload = foundAdmin;
                    ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    LogManager.LogInfo("Record not found." + _admin.AdminId);
                    ApiResp.Message = "Record not found.";
                    ApiResp.Status = "Failed";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (SqlException ex)
            {
                LogManager.LogInfo("-- Error::AdminRepository::EditAdminProfile --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                ApiResp.Message = "Something went wrong!";
                ApiResp.Status = "Internal Server Error";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }

            return ApiResp;
        }

        public APIResponse ChangeAdminStatus(ChangeRecordStatus _admin, string auth)
        {
            APIResponse aResp = new APIResponse();

            LogManager.LogInfo("-- Run::AdminRepository::ChangeAdminStatus --");
            LogManager.LogDebugObject(_admin);

            try
            {
                Admin foundAdmin = DbContext.Admins.Where(a => a.Id == _admin.RecordId).FirstOrDefault();
                if (foundAdmin != null)
                {
                    DateTime NowDate = DateTime.Now;
                    foundAdmin.LastEditedDate = DateTime.Now;
                    foundAdmin.DateEnabled = DateTime.Now;
                    foundAdmin.IsEnabled = _admin.IsActive;

                    DbContext.Admins.Update(foundAdmin);
                    DbContext.SaveChanges();
                    aResp = new APIResponse
                    {
                        Message = "Account updated successfully.",
                        Status = "Success!",
                        Payload = foundAdmin,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Record not found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::AdminRepository::ChangeAdminStatus --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse ForgotPassword(AdminForgotPassword _admin, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::AdminRepository::ForgotPassword --");
            LogManager.LogDebugObject(_admin);

            try
            {
                FacilityUser facilityUser = DbContext.FacilityUsers.AsNoTracking().FirstOrDefault(u => u.Email.ToLower() == _admin.Email.ToLower());
                if (facilityUser == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Account not found.",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                if (facilityUser.IsEnabled == false)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Your account is inactive. Contact the support team.",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                //send new password
                var sysGenPassword = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
                facilityUser.FirstName = facilityUser.FirstName != null ? facilityUser.FirstName.ToUpper() : facilityUser.FirstName = "";
                facilityUser.LastName = facilityUser.LastName != null ? facilityUser.LastName.ToUpper() : facilityUser.LastName = "";
                var emailBody = String.Format(APIConfig.MsgConfigs.ForgotPassword, $"{facilityUser.FirstName.ToUpper()} {facilityUser.LastName.ToUpper()}", sysGenPassword);

                var EmailParam = _conf.MailConfig;
                EmailParam.To = new List<string>() { facilityUser.Email };
                EmailParam.Subject = "Sidekick Admin: Forgot your password.";
                EmailParam.Body = emailBody;

                EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(_conf, EmailParam.Body, LogManager, false);
                var sendStatus = SendEmailByEmailAddress(new List<string>() { facilityUser.Email }, EmailParam, LogManager);

                if (sendStatus == 0)
                {
                    facilityUser.Password = facilityUser.HashP(sysGenPassword.ToString(), APIConfig.TokenKeys.Key);
                    facilityUser.LastEditedBy = facilityUser.FacilityUserId;
                    facilityUser.LastEditedDate = DateTime.Now;
                    facilityUser.IsLocked = true;
                    facilityUser.LockedDateTime = DateTime.Now;

                    DbContext.FacilityUsers.Update(facilityUser);
                    DbContext.SaveChanges();

                    apiResp = new APIResponse
                    {
                        Message = "You will receive an email containing your new password.",
                        Status = "Success",
                        Payload = facilityUser.FacilityUserType,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::AdminRepository::ForgotPassword --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }
    }
}
