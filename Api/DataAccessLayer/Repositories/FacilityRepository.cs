
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class FacilityRepository : APIBaseRepo, IFacilityRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public FacilityRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public async Task<APIResponse> AddFacility(string _auth, FacilityProfile _facility, IMainHttpClient _mhttpc = null, APIConfigurationManager _conf = null)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FacilityRepository::AddFacility --");
            LogManager.LogDebugObject(_facility);

            try
            {
                AdminLoginTransaction IsUserLoggedIn = await DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefaultAsync(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                if (_facility.FacilityId == Guid.Empty)
                {
                    var allFacilities = await DbContext.Facilities.ToListAsync();
                    Facility facility = await DbContext.Facilities.Where(u => u.Name.ToLower() == _facility.Name.ToLower()).FirstOrDefaultAsync();
                    if (facility == null)
                    {
                        var isExistingEmail = allFacilities.Where(f => f.Email.ToLower() == _facility.Email.ToLower()).ToList();
                        var checkEmailExistingInAdmin = await DbContext.FacilityUsers.Where(a => a.Email.ToLower() == _facility.Email.ToLower()).ToListAsync();
                        if (isExistingEmail.Any() || checkEmailExistingInAdmin.Any())
                        {
                            return apiResp = new APIResponse
                            {
                                Message = "Registration Failed. Duplicate email found.",
                                Status = "Success!",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }

                        Guid GuidId = Guid.NewGuid();
                        DateTime TodaysDate = DateTime.Now;
                        var getArea = await DbContext.Areas.Where(a => a.AreaId == _facility.AreaId).FirstOrDefaultAsync();

                        Facility newFacility = new Facility
                        {
                            LastEditedBy = IsUserLoggedIn.AdminId,
                            LastEditedDate = TodaysDate,
                            CreatedBy = IsUserLoggedIn.AdminId,
                            CreatedDate = TodaysDate,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.AdminId,
                            DateEnabled = TodaysDate,
                            IsLocked = false,
                            LockedDateTime = TodaysDate,

                            FacilityId = GuidId,
                            Name = _facility.Name,
                            Description = _facility.Description,
                            OwnerFirstName = _facility.OwnerFirstName,
                            OwnerLastName = _facility.OwnerLastName,
                            MobileNumber = _facility.MobileNumber,
                            Email = _facility.Email,
                            ImageUrl = string.IsNullOrWhiteSpace(_facility.ImageUrl) ? APIConfig.HostURL + "/resources/Defaults/Facility_Default_Logo.jpg" : _facility.ImageUrl,
                            IsEveryday = _facility.IsEveryday,
                            IsHalfHourAllowed = _facility.IsHalfHourAllowed,
                            Area = getArea,
                            Street = _facility.Street,
                            AreaName = _facility.AreaName,
                            Latitude = _facility.Latitude,
                            Longitude = _facility.Longitude
                        };

                        var facilitySports = await DbContext.FacilitySports.Where(f => f.FacilityId == newFacility.FacilityId).ToListAsync();
                        var facilitySportList = new List<FacilitySport>();
                        if (facilitySports.Count == 0)
                        {
                            foreach (var facilitySport in _facility.FacilitySports)
                            {
                                facilitySportList.Add(new FacilitySport
                                {
                                    FacilityId = newFacility.FacilityId,
                                    SportId = facilitySport.SportId,
                                    LastEditedBy = IsUserLoggedIn.AdminId,
                                    LastEditedDate = DateTime.Now,
                                    IsEnabledBy = IsUserLoggedIn.AdminId,
                                    IsEnabled = true,
                                    DateEnabled = DateTime.Now,
                                    CreatedBy = IsUserLoggedIn.AdminId,
                                    CreatedDate = DateTime.Now,
                                    IsLocked = false,
                                    LockedDateTime = DateTime.Now
                                });
                            }
                        }

                        DbContext.FacilitySports.AddRange(facilitySportList);

                        var facilityTimings = await DbContext.FacilityTimings.Where(f => f.FacilityId == newFacility.FacilityId).ToListAsync();
                        var facilityTimingList = new List<FacilityTiming>();
                        if (facilityTimings.Count == 0)
                        {
                            if (_facility.IsEveryday)
                            {
                                _facility.FacilityTimings.Add(new FacilityTiming
                                {
                                    Day = 0,
                                    TimeStart = _facility.TimeStart,
                                    TimeEnd = _facility.TimeEnd
                                });
                            }

                            foreach (var facilityTiming in _facility.FacilityTimings)
                            {
                                facilityTimingList.Add(new FacilityTiming
                                {
                                    FacilityId = newFacility.FacilityId,
                                    Day = facilityTiming.Day,
                                    TimeStart = _facility.IsEveryday ? _facility.TimeStart : facilityTiming.TimeStart,
                                    TimeEnd = _facility.IsEveryday ? _facility.TimeEnd : facilityTiming.TimeEnd,
                                    IsEveryday = _facility.IsEveryday,

                                    LastEditedBy = IsUserLoggedIn.AdminId,
                                    LastEditedDate = DateTime.Now,
                                    IsEnabledBy = IsUserLoggedIn.AdminId,
                                    IsEnabled = true,
                                    DateEnabled = DateTime.Now,
                                    CreatedBy = IsUserLoggedIn.AdminId,
                                    CreatedDate = DateTime.Now,
                                    IsLocked = false,
                                    LockedDateTime = DateTime.Now
                                });
                            }
                        }
                        DbContext.FacilityTimings.AddRange(facilityTimingList);

                        //send password via email for testing initial setup
                        var EmailParam = _conf.MailConfig;
                        EmailParam.To = new List<string>() { _facility.Email };
                        EmailParam.Subject = APIConfig.MsgConfigs.RegisterFacilityUserEmailSubject;

                        EmailParam.Body = String.Format(APIConfig.MsgConfigs.RegisterFacilityUser, _facility.OwnerFirstName, _facility.Email, _facility.Password);

                        EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(_conf, EmailParam.Body, LogManager,false);

                        var sendStatus = EmailHelper.SendEmailByEmailAddress(new List<string>() { _facility.Email }, EmailParam, LogManager);

                        if (sendStatus == 0)
                        {
                            //add initial facility records
                            var facilityOwnerId = AddFacilityOwner(_facility, IsUserLoggedIn.AdminId, newFacility.FacilityId);
                            newFacility.FacilityOwnerId = facilityOwnerId;
                            DbContext.Facilities.Add(newFacility);

                            await DbContext.SaveChangesAsync();

                            return apiResp = new APIResponse
                            {
                                Message = newFacility.Name.ToUpper() + " registered successfully.",
                                Status = "Success!",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            return apiResp = new APIResponse
                            {
                                Message = "Registration failed.",
                                Status = "Failed",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                    }
                    else
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Registration Failed. Duplicate facility name found.",
                            Status = "Failed!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }

                var existingFacility = await DbContext.Facilities.Where(f => f.FacilityId == _facility.FacilityId).FirstOrDefaultAsync();
                if (existingFacility != null)
                {
                    var getArea = DbContext.Areas.Where(a => a.AreaId == _facility.AreaId).FirstOrDefault();

                    existingFacility.LastEditedBy = IsUserLoggedIn.AdminId;
                    existingFacility.LastEditedDate = DateTime.Now;
                    existingFacility.IsEnabled = true;

                    existingFacility.Name = _facility.Name;
                    existingFacility.Description = _facility.Description;
                    existingFacility.MobileNumber = _facility.MobileNumber;
                    var oldEmail = existingFacility.Email;
                    existingFacility.Email = _facility.Email;
                    existingFacility.ImageUrl = _facility.ImageUrl;
                    existingFacility.IsEveryday = _facility.IsEveryday;
                    existingFacility.IsHalfHourAllowed = _facility.IsHalfHourAllowed;
                    existingFacility.Area = getArea;
                    existingFacility.Street = _facility.Street;
                    existingFacility.AreaName = _facility.AreaName;
                    existingFacility.Area.AreaId = _facility.AreaId;
                    existingFacility.Longitude = _facility.Longitude;
                    existingFacility.Latitude = _facility.Latitude;
                    existingFacility.OwnerFirstName = _facility.OwnerFirstName;
                    existingFacility.OwnerLastName = _facility.OwnerLastName;

                    DbContext.Facilities.Update(existingFacility);

                    if (!string.IsNullOrWhiteSpace(_facility.Password))
                    {
                        var facilityUser = await DbContext.FacilityUsers.Where(f => f.FacilityId == _facility.FacilityId
                                                                    && f.FacilityUserType == EFacilityUserType.OWNER
                                                                    && f.FacilityUserId == _facility.FacilityOwnerId).FirstOrDefaultAsync();

                        if (facilityUser != null)
                        {
                            facilityUser.Password = facilityUser.HashP(_facility.Password, APIConfig.TokenKeys.Key);
                            facilityUser.LastEditedDate = DateTime.Now;
                            facilityUser.Email = _facility.Email;
                        }

                        DbContext.FacilityUsers.Update(facilityUser);
                    }

                    UpdateFacilitySports(existingFacility.FacilityId, IsUserLoggedIn.AdminId, _facility.FacilitySports);
                    UpdateFacilityTimings(IsUserLoggedIn.AdminId, _facility, existingFacility);

                    DbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = existingFacility.Name.ToUpper() + " updated successfully.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::AddFacility --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        private void UpdateFacilityTimings(Guid adminId, FacilityProfile _facility, Facility existingFacility)
        {
            var newFacilityTimings = new List<FacilityTiming>();
            var existingFacilityTimings = DbContext.FacilityTimings.Where(f => f.FacilityId == existingFacility.FacilityId).ToList();
            if (existingFacilityTimings.Count != 0)
            {
                DbContext.FacilityTimings.RemoveRange(existingFacilityTimings);
            }

            if (_facility.FacilityTimings.Count != 0)
            {
                foreach (var item in _facility.FacilityTimings)
                {
                    newFacilityTimings.Add(new FacilityTiming
                    {
                        FacilityId = existingFacility.FacilityId,
                        Day = item.Day,
                        TimeStart = item.TimeStart,
                        TimeEnd = item.TimeEnd,
                        IsEveryday = item.IsEveryday,

                        LastEditedBy = adminId,
                        LastEditedDate = DateTime.Now,
                        IsEnabledBy = adminId,
                        IsEnabled = true,
                        DateEnabled = DateTime.Now,
                        CreatedBy = adminId,
                        CreatedDate = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now
                    });
                }

                DbContext.FacilityTimings.AddRange(newFacilityTimings);
            }

            if (_facility.IsEveryday)
            {
                newFacilityTimings.Add(new FacilityTiming
                {
                    FacilityId = existingFacility.FacilityId,
                    TimeStart = _facility.TimeStart,
                    TimeEnd = _facility.TimeEnd,
                    IsEveryday = _facility.IsEveryday,

                    LastEditedBy = adminId,
                    LastEditedDate = DateTime.Now,
                    IsEnabledBy = adminId,
                    IsEnabled = true,
                    DateEnabled = DateTime.Now,
                    CreatedBy = adminId,
                    CreatedDate = DateTime.Now,
                    IsLocked = false,
                    LockedDateTime = DateTime.Now
                });

                DbContext.FacilityTimings.AddRange(newFacilityTimings);
            }
        }

        private void UpdateFacilitySports(Guid facilityId, Guid adminId, List<FacilitySportDto> facilitySports)
        {
            var newFacilitySports = new List<FacilitySport>();
            var existingFacilitySports = DbContext.FacilitySports.Where(f => f.FacilityId == facilityId).ToList();
            if (existingFacilitySports.Count != 0)
            {
                DbContext.FacilitySports.RemoveRange(existingFacilitySports);
            }

            if (facilitySports.Count != 0)
            {
                foreach (var item in facilitySports)
                {
                    newFacilitySports.Add(new FacilitySport
                    {
                        FacilityId = facilityId,
                        SportId = item.SportId,
                        LastEditedBy = adminId,
                        LastEditedDate = DateTime.Now,
                        IsEnabledBy = adminId,
                        IsEnabled = true,
                        DateEnabled = DateTime.Now,
                        CreatedBy = adminId,
                        CreatedDate = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now
                    });
                }

                DbContext.FacilitySports.AddRange(newFacilitySports);
            }
        }

        private Guid AddFacilityOwner(FacilityProfile _user, Guid _userLoggedIn, Guid _newFacilityId)
        {
            LogManager.LogInfo("-- Run::FacilityRepository::AddFacilityOwner --");

            FacilityUser facilityUser = DbContext.FacilityUsers.Where(u => u.Email.ToLower() == _user.Email.ToLower()).FirstOrDefault();
            if (facilityUser == null)
            {
                Guid GuidId = Guid.NewGuid();
                DateTime CreatedDate = DateTime.Now;

                FacilityUser newUser = new FacilityUser
                {
                    FacilityId = _newFacilityId,
                    FacilityUserId = GuidId,
                    FacilityUserType = EFacilityUserType.OWNER,
                    FacilityAccountType = EAccountType.FACILITYUSER,

                    Email = _user.Email,
                    FirstName = _user.OwnerFirstName,
                    LastName = _user.OwnerLastName,
                    MobileNumber = _user.MobileNumber,
                    DevicePlatform = EDevicePlatform.Web,

                    LastEditedBy = _userLoggedIn,
                    LastEditedDate = CreatedDate,
                    CreatedBy = _userLoggedIn,
                    CreatedDate = CreatedDate,
                    IsEnabled = true,
                    IsEnabledBy = _userLoggedIn,
                    DateEnabled = CreatedDate,
                    IsLocked = true, //TO Clarify: Autoverified??
                    LockedDateTime = CreatedDate
                };
                newUser.Password = newUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key);

                DbContext.FacilityUsers.Add(newUser);
                DbContext.SaveChanges();

                return newUser.FacilityUserId;
            }
            else
            {
                LogManager.LogInfo("-- Error::FacilityRepository::AddFacilityOwner >> Duplicate email found. --" + _user.Email);
                return Guid.Empty;
            }
        }

        public APIResponse GetAllFacilities()
        {
            APIResponse aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FacilityRepository::GetAllFacilities --");

            try
            {
                IEnumerable<FacilityList> getAllFacilities = null;
                getAllFacilities = DbContext.Facilities.Where(f => f.IsEnabled == true).AsNoTracking()
                    .Select(i => new FacilityList
                    {
                        FacilityId = i.FacilityId,
                        Name = i.Name,
                        Email = i.Email,
                        Location = i.Street + " " + i.Area.AreaName + " " + i.City + " " + i.Country,
                        CreatedDate = i.CreatedDate,
                        IsEnabled = i.IsEnabled,
                        FacilityImage = i.ImageUrl,
                        DateUpdated = i.LastEditedDate.Value
                    }).ToList();

                //TO DO: Where to get values of these??
                if (getAllFacilities.Count() > 0)
                {
                    foreach (var facility in getAllFacilities)
                    {
                        int totalPitch = DbContext.FacilityPitches.Where(x => x.FacilityId == facility.FacilityId && x.IsEnabled == true).Count();
                        facility.PitchNo = totalPitch;
                    }

                    return aResp = new APIResponse
                    {
                        Message = "All records found.",
                        Status = "Success!",
                        Payload = getAllFacilities,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return aResp = new APIResponse
                    {
                        Message = "No records found.",
                        Status = "Success!",
                        Payload = getAllFacilities,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::GetAllFacilities --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetFacilityProfile(Guid _facilityId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("-- Run::FacilityRepository::GetFacilityProfile --");
            LogManager.LogDebugObject(_facilityId);

            try
            {
                Facility facility = DbContext.Facilities.AsNoTracking().Where(x => x.FacilityId == _facilityId).Include(f => f.Area).FirstOrDefault();
                var areas = DbContext.Areas.AsNoTracking().Where(a => a.IsEnabled == true).ToList();
                if (facility != null)
                {
                    FacilityProfile profile = new FacilityProfile
                    {
                        FacilityId = facility.FacilityId,
                        Name = facility.Name,
                        Description = facility.Description,
                        OwnerFirstName = facility.OwnerFirstName,
                        OwnerLastName = facility.OwnerLastName,
                        Email = facility.Email,
                        MobileNumber = facility.MobileNumber,
                        ImageUrl = facility.ImageUrl,
                        IsEveryday = facility.IsEveryday,
                        Street = facility.Street,
                        AreaName = facility.AreaName,
                        Latitude = facility.Latitude,
                        Longitude = facility.Longitude,
                        Areas = areas,
                        AreaId = facility.Area.AreaId,
                        IsHalfHourAllowed = facility.IsHalfHourAllowed,
                        FacilityOwnerId = facility.FacilityOwnerId
                    };

                    FacilityUser owner = DbContext.FacilityUsers.Where(x => x.FacilityId == _facilityId && x.FacilityUserType == EFacilityUserType.OWNER).FirstOrDefault();
                    if (owner != null)
                    {
                        profile.Password = owner.Password;
                    }

                    List<FacilityUser> staffList = DbContext.FacilityUsers.Where(x => x.FacilityId == _facilityId && x.FacilityUserType != EFacilityUserType.OWNER).ToList();
                    if (staffList != null)
                    {
                        List<FacilityStaff> facilityStaffs = new List<FacilityStaff>();
                        foreach (var staff in staffList)
                        {
                            FacilityStaff staffProfile = new FacilityStaff
                            {
                                FacilityId = staff.FacilityId,
                                FirstName = staff.FirstName,
                                LastName = staff.LastName,
                                FacilityUserType = staff.FacilityUserType,
                                Email = staff.Email
                            };

                            facilityStaffs.Add(staffProfile);
                        }

                        profile.FacilityStaffs = facilityStaffs;
                    }

                    if (facility.IsEveryday)
                    {
                        var facilityHour = DbContext.FacilityTimings.Where(x => x.FacilityId == _facilityId && x.IsEnabled == true && x.IsEveryday == true).FirstOrDefault();
                        if (facilityHour != null)
                        {
                            var facilityHours = new FacilityHour
                            {
                                TimeStart = facilityHour.TimeStart,
                                TimeEnd = facilityHour.TimeEnd
                            };

                            profile.FacilityHours = facilityHours;
                        }

                        profile.FacilityTimings = new List<FacilityTiming>();
                    }
                    else
                    {
                        var facilityTimings = DbContext.FacilityTimings.Where(x => x.FacilityId == _facilityId && x.IsEnabled == true && x.IsEveryday == false).ToList();
                        profile.FacilityTimings = facilityTimings.Count == 0 ? new List<FacilityTiming>() : facilityTimings;
                    }

                    List<FacilitySport> sportsList = DbContext.FacilitySports.Where(x => x.FacilityId == _facilityId).ToList();
                    if (sportsList != null)
                    {
                        List<FacilitySportDto> facilitySports = new List<FacilitySportDto>();
                        foreach (var item in sportsList)
                        {
                            var selected = DbContext.Sports.Where(s => s.SportId == item.SportId).FirstOrDefault();
                            string sportName = selected == null ? "" : selected.Name;
                            FacilitySportDto sport = new FacilitySportDto
                            {
                                SportId = item.SportId,
                                FacilityId = item.FacilityId,
                                Name = sportName
                            };

                            facilitySports.Add(sport);
                        }

                        profile.FacilitySports = facilitySports;
                    }

                    return aResp = new APIResponse
                    {
                        Message = "Record found.",
                        Status = "Success!",
                        Payload = profile,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return aResp = new APIResponse
                    {
                        Message = "Record not found.",
                        Status = "Success!",
                        Payload = null,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::GetFacilityProfile --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse EditFacilityProfile(FacilityProfile _facility)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FacilityRepository::EditFacilityUser --");
            LogManager.LogDebugObject(_facility);

            try
            {
                Facility facility = DbContext.Facilities.Where(u => u.FacilityId == _facility.FacilityId).FirstOrDefault();
                if (facility != null)
                {
                    Facility hasDuplicate = DbContext.Facilities.Where(u => u.Name == _facility.Name).FirstOrDefault();
                    if (hasDuplicate != null && hasDuplicate.FacilityId == facility.FacilityId)
                    {
                        DateTime DateUpdated = DateTime.Now;

                        facility.LastEditedBy = _facility.UserLoggedIn;
                        facility.LastEditedDate = DateUpdated;
                        facility.IsEnabledBy = _facility.UserLoggedIn;
                        facility.DateEnabled = DateUpdated;

                        facility.Name = _facility.Name;
                        facility.Description = _facility.Description;
                        facility.OwnerFirstName = _facility.OwnerFirstName;
                        facility.OwnerLastName = _facility.OwnerLastName;
                        facility.Email = _facility.Email;
                        facility.MobileNumber = _facility.MobileNumber;
                        facility.ImageUrl = _facility.ImageUrl;
                        facility.IsEveryday = _facility.IsEveryday;
                        facility.IsHalfHourAllowed = _facility.IsHalfHourAllowed;
                        facility.Street = _facility.Street;
                        facility.AreaName = _facility.AreaName;
                        facility.Latitude = _facility.Latitude;
                        facility.Longitude = _facility.Longitude;

                        DbContext.Facilities.Update(facility);                     
                        DbContext.SaveChangesAsync();

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
                            Message = "Processing Failed. Duplicate record found.",
                            Status = "Failed!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Processing Failed. Record not found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::EditFacilityUser --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        private static void AddNewFacilityTimings(FacilityProfile _facility, DateTime DateUpdated, List<FacilityTiming> facilityTimingList)
        {
            if (_facility.FacilityTimings.Count == 0)
            {
                facilityTimingList.Add(new FacilityTiming
                {
                    FacilityId = _facility.FacilityId,
                    IsEveryday = _facility.IsEveryday,
                    TimeStart = _facility.TimeStart,
                    TimeEnd = _facility.TimeEnd,
                    LastEditedBy = _facility.UserLoggedIn,
                    LastEditedDate = DateUpdated,
                    IsEnabledBy = _facility.UserLoggedIn,
                    IsEnabled = true,
                    DateEnabled = DateUpdated,
                    CreatedBy = _facility.UserLoggedIn,
                    CreatedDate = DateUpdated,
                    IsLocked = false,
                    LockedDateTime = DateUpdated
                });
            }
            else
            {
                foreach (var item in _facility.FacilityTimings)
                {
                    facilityTimingList.Add(new FacilityTiming
                    {
                        FacilityId = _facility.FacilityId,
                        Day = item.Day,
                        IsEveryday = item.IsEveryday,
                        TimeStart = _facility.IsEveryday ? _facility.TimeStart : item.TimeStart,
                        TimeEnd = _facility.IsEveryday ? _facility.TimeEnd : item.TimeEnd,
                        LastEditedBy = _facility.UserLoggedIn,
                        LastEditedDate = DateUpdated,
                        IsEnabledBy = _facility.UserLoggedIn,
                        IsEnabled = true,
                        DateEnabled = DateUpdated,
                        CreatedBy = _facility.UserLoggedIn,
                        CreatedDate = DateUpdated,
                        IsLocked = false,
                        LockedDateTime = DateUpdated
                    });
                }
            }
        }

        private static void AddNewFacilitySports(FacilityProfile _facility, DateTime DateUpdated, List<FacilitySport> sportList)
        {
            foreach (var item in _facility.FacilitySports)
            {
                sportList.Add(new FacilitySport
                {
                    FacilityId = _facility.FacilityId,
                    SportId = item.SportId,
                    LastEditedBy = _facility.UserLoggedIn,
                    LastEditedDate = DateUpdated,
                    IsEnabledBy = _facility.UserLoggedIn,
                    IsEnabled = true,
                    DateEnabled = DateUpdated,
                    CreatedBy = _facility.UserLoggedIn,
                    CreatedDate = DateUpdated,
                    IsLocked = false,
                    LockedDateTime = DateUpdated
                });
            }
        }

        public async Task<APIResponse> FacilityChangeStatus(string _auth, ChangeStatus _facility)
        {
            var apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FacilityRepository::FacilityChangeStatus --");
            LogManager.LogDebugObject(_facility);

            try
            {
                var IsUserLoggedIn = DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                var facility = await DbContext.Facilities.Where(a => a.FacilityId == _facility.GuID).FirstOrDefaultAsync();
                if (facility != null)
                {
                    var hasBookings = await DbContext.UserPitchBookings.Where(u => u.FacilityId == _facility.GuID).ToListAsync();
                    if (hasBookings.Any())
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "The Facility still has ongoing Bookings. Cannot be Deleted",
                            Status = "Cancelled",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    DateTime DateUpdated = DateTime.Now;

                    facility.LastEditedBy = IsUserLoggedIn.AdminId;
                    facility.LastEditedDate = DateUpdated;
                    facility.IsEnabled = _facility.IsEnabled;
                    facility.IsEnabledBy = IsUserLoggedIn.AdminId;
                    facility.DateEnabled = DateUpdated;

                    DbContext.Facilities.Update(facility);
                    await DbContext.SaveChangesAsync();

                    return apiResp = new APIResponse
                    {
                        Message = "Record updated successfully.",
                        Status = "Success",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                var facilityUsers = await DbContext.FacilityUsers.Where(f => f.FacilityId == _facility.GuID).ToListAsync();
                var toBeDeactivated = new List<FacilityUser>();
                if (facilityUsers.Any())
                {
                    foreach (var facilityUser in facilityUsers)
                    {
                        facilityUser.LastEditedBy = IsUserLoggedIn.AdminId;
                        facilityUser.LastEditedDate = DateTime.Now;
                        facilityUser.IsEnabled = _facility.IsEnabled;
                        facilityUser.IsEnabledBy = IsUserLoggedIn.AdminId;

                        toBeDeactivated.Add(facilityUser);
                    }

                    DbContext.FacilityUsers.UpdateRange(toBeDeactivated);
                }

                return apiResp = new APIResponse
                {
                    Message = "Processing Failed. Record not found.",
                    Status = "Failed!",
                    StatusCode = System.Net.HttpStatusCode.NotFound
                };

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::FacilityChangeStatus --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse UpdateFacilityProfile(FacilityProfile _facility)
        {
            APIResponse apiResp = new();

            LogManager.LogInfo("-- Run::FacilityRepository::UpdateFacilityProfile--");
            LogManager.LogDebugObject(_facility);

            try
            {
                Facility facility = DbContext.Facilities.Where(u => u.FacilityId == _facility.FacilityId).Include(u => u.Area).FirstOrDefault();
                if (facility != null)
                {
                    DateTime DateUpdated = DateTime.Now;

                    facility.LastEditedBy = _facility.UserLoggedIn;
                    facility.LastEditedDate = DateUpdated;
                    facility.IsEnabledBy = _facility.UserLoggedIn;
                    facility.DateEnabled = DateUpdated;

                    facility.Name = _facility.Name;
                    facility.Description = _facility.Description;
                    facility.Email = _facility.Email;
                    facility.MobileNumber = _facility.MobileNumber;
                    facility.ImageUrl = _facility.ImageUrl;
                    facility.IsEveryday = _facility.IsEveryday;
                    facility.IsHalfHourAllowed = _facility.IsHalfHourAllowed;
                    facility.Street = _facility.Street;
                    facility.OwnerFirstName = _facility.OwnerFirstName;
                    facility.OwnerLastName = _facility.OwnerLastName;

                    var area = DbContext.Areas.Where(a => a.AreaId == _facility.AreaId).FirstOrDefault();
                    facility.Area = area;

                    DbContext.Facilities.Update(facility);

                    var facilityTimings = DbContext.FacilityTimings.Where(f => f.FacilityId == _facility.FacilityId && f.IsEnabled == true).ToList();
                    var facilityTimingList = new List<FacilityTiming>();
                    if (facilityTimings != null)
                    {
                        DbContext.FacilityTimings.RemoveRange(facilityTimings);
                    }

                    AddNewFacilityTimings(_facility, DateUpdated, facilityTimingList);

                    DbContext.FacilityTimings.AddRange(facilityTimingList);

                    var facilitySports = DbContext.FacilitySports.Where(s => s.FacilityId == _facility.FacilityId && s.IsEnabled == true).ToList();
                    var sportList = new List<FacilitySport>();
                    if (facilitySports != null)
                    {
                        DbContext.FacilitySports.RemoveRange(facilitySports);
                    }

                    AddNewFacilitySports(_facility, DateUpdated, sportList);

                    DbContext.FacilitySports.AddRange(sportList);

                    DbContext.SaveChanges();
                    return apiResp = new APIResponse
                    {
                        Message = "Record updated successfully.",
                        Status = "Success!",
                        Payload = facility,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Processing Failed. Record not found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::UpdateFacilityProfile --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;


        }

        public APIResponse GetAreas()
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FacilityRepository::GetAreas --");

            try
            {
                var areas = DbContext.Areas.ToList();
                return aResp = new APIResponse
                {
                    Message = "All records found.",
                    Status = "Success!",
                    Payload = areas,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::GetAreas --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetFacilityUserTypes()
        {
            APIResponse aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FacilityRepository::GetFacilityUserTypes --");

            try
            {
                IEnumerable<FacilityUserType> getAllUserTypes = null;
                getAllUserTypes = DbContext.FacilityUserTypes.AsNoTracking()
                    .Where(x => x.IsEnabled == true)
                    .Select(i => new FacilityUserType
                    {
                        FacilityRoleId = i.FacilityRoleId,
                        FacilityRoleName = i.FacilityRoleName
                    }).ToList();

                if (getAllUserTypes.Count() > 0)
                {
                    return aResp = new APIResponse
                    {
                        Message = "All records found.",
                        Status = "Success!",
                        Payload = getAllUserTypes,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return aResp = new APIResponse
                    {
                        Message = "No records found.",
                        Status = "Success!",
                        Payload = getAllUserTypes,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::GetFacilityUserTypes --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse AddEditFacilityUserType(FacilityUserType _facilityUserType)
        {
            APIResponse aResp = new();
            try
            {

                FacilityUserType facilityUserType = DbContext.FacilityUserTypes.Where(u => u.FacilityRoleId == _facilityUserType.FacilityRoleId && u.IsEnabled == true).FirstOrDefault();
                if (facilityUserType != null)
                {
                    LogManager.LogInfo("-- Run::FacilityRepository::UpdateFacilityUserType --");

                    //facilityUserType.FacilityRoleId = _facilityUserType.FacilityRoleId;
                    facilityUserType.FacilityRoleName = _facilityUserType.FacilityRoleName;

                    DbContext.FacilityUserTypes.Update(facilityUserType);
                    DbContext.SaveChanges();

                    return aResp = new APIResponse
                    {
                        Message = "Facility admin role updated successfully!",
                        Status = "Success!",
                        Payload = facilityUserType,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    FacilityUserType facilityRoleName = DbContext.FacilityUserTypes.Where(u => u.FacilityRoleName == _facilityUserType.FacilityRoleName && u.IsEnabled == true).FirstOrDefault();
                    if (facilityRoleName != null)
                    {

                        return aResp = new APIResponse
                        {
                            Message = "Facility admin role already exists!",
                            Status = "Error!",
                            Payload = _facilityUserType,
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    else
                    {
                        LogManager.LogInfo("-- Run::FacilityRepository::AddFacilityUserType --");

                        FacilityUserType newUserType = new()
                        {
                            FacilityRoleId = Guid.NewGuid(),
                            FacilityRoleName = _facilityUserType.FacilityRoleName
                        };
                        DbContext.FacilityUserTypes.Add(newUserType);
                        DbContext.SaveChanges();

                        return aResp = new APIResponse
                        {
                            Message = "Facility admin role added successfully!",
                            Status = "Success!",
                            Payload = newUserType,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::AddEditFacilityUserType --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse DeleteFacilityUserType(FacilityUserType _facilityUserType)
        {
            APIResponse aResp = new();
            try
            {

                FacilityUserType facilityUserType = DbContext.FacilityUserTypes.Where(u => u.FacilityRoleId == _facilityUserType.FacilityRoleId && u.FacilityRoleName == _facilityUserType.FacilityRoleName).FirstOrDefault();
                if (facilityUserType != null)
                {
                    LogManager.LogInfo("-- Run::FacilityRepository::DeleteFacilityUserType --");

                    facilityUserType.IsEnabled = false;

                    DbContext.FacilityUserTypes.Update(facilityUserType);
                    DbContext.SaveChanges();

                    return aResp = new APIResponse
                    {
                        Message = "Facility admin role deleted successfully!",
                        Status = "Success!",
                        Payload = facilityUserType,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return aResp = new APIResponse
                    {
                        Message = "Facility admin role not found!",
                        Status = "Error!",
                        Payload = null,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FacilityRepository::DeleteFacilityUserType --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public async Task<Facility> GetFacility(Guid FacilityId)
        {
            return await DbContext.Facilities.AsNoTracking().FirstOrDefaultAsync(x => x.FacilityId == FacilityId);
        }
    }
}

