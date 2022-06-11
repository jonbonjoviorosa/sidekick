using Microsoft.EntityFrameworkCore;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.Specialty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class SpecialtyRepository: APIBaseRepo, ISpecialtyRepository
    {
        private readonly APIDBContext context;
        ILoggerManager LogManager { get; }

        public SpecialtyRepository(APIDBContext context,
            ILoggerManager _logManager)
        {
            LogManager = _logManager;
            this.context = context;
        }

        public async Task<APIResponse<IEnumerable<Specialty>>> GetSpecialties()
        {
            APIResponse<IEnumerable<Specialty>> apiResp = new APIResponse<IEnumerable<Specialty>>();

            LogManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var specialties = await context.Specialties.Where(s => s.IsEnabled == true).ToListAsync();
                return apiResp = new APIResponse<IEnumerable<Specialty>>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = specialties
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = Messages.SomethingWentWrong;
                apiResp.Status = Status.InternalServerError;
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }

            return apiResp;
        }

        ///<inheritdoc/>
        public APIResponse AddOrEditSpecialty(string _auth, Specialty specialty)
        {
            var apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::SpecialtyRepository::AddOrEditSpecialty --");
            LogManager.LogDebugObject(specialty);

            try
            {
                var IsUserLoggedIn = context.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.Unauthorized
                    };
                }
                var specialties = context.Specialties.Where(s => s.IsEnabled == true);
                var isSpecialtyExisting = specialties.Where(e => e.SpecialtyId == specialty.SpecialtyId).FirstOrDefault();
                var todaysDate = DateTime.Now;
                if (isSpecialtyExisting == null)
                {
                    if (!specialties.Where(e => e.Name == specialty.Name).Any())
                    {
                        var newSpecialty = new Specialty
                        {
                            LastEditedBy = IsUserLoggedIn.AdminId,
                            LastEditedDate = todaysDate,
                            CreatedBy = IsUserLoggedIn.AdminId,
                            CreatedDate = todaysDate,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.AdminId,
                            DateEnabled = todaysDate,
                            IsLocked = false,
                            LockedDateTime = todaysDate,
                            SpecialtyId = Guid.NewGuid(),
                            Name = specialty.Name,
                            Icon = specialty.Icon,
                            Description = specialty.Name + " Description"
                        };

                        context.Specialties.Add(newSpecialty);
                        context.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "New Specialty added successfully.",
                            Status = "Success!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    return ResponseFailed(out apiResp);
                }
                else if (!string.IsNullOrWhiteSpace(isSpecialtyExisting.SpecialtyId.ToString()))
                {
                    if(specialties.Where(s => s.Name == specialty.Name).Any())
                    {
                        return ResponseFailed(out apiResp);
                    }
                    isSpecialtyExisting.LastEditedBy = IsUserLoggedIn.AdminId;
                    isSpecialtyExisting.LastEditedDate = todaysDate;
                    isSpecialtyExisting.CreatedBy = IsUserLoggedIn.AdminId;
                    isSpecialtyExisting.CreatedDate = todaysDate;
                    isSpecialtyExisting.IsEnabled = true;
                    isSpecialtyExisting.IsEnabledBy = IsUserLoggedIn.AdminId;
                    isSpecialtyExisting.DateEnabled = todaysDate;
                    isSpecialtyExisting.IsLocked = false;
                    isSpecialtyExisting.LockedDateTime = todaysDate;
                    isSpecialtyExisting.Name = specialty.Name;
                    isSpecialtyExisting.Icon = specialty.Icon;

                    context.Specialties.Update(isSpecialtyExisting);
                    context.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Updated Specialty successfully.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }
                else
                {
                    return ResponseFailed(out apiResp);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::SpecialtyRepository::AddOrEditSpecialty--");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        ///<inheritdoc/>
        public APIResponse DeleteSpecialty(string _auth, Guid specialtyId)
        {
            var apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::SpecialtyRepository::DeleteSpecialty --");
            LogManager.LogDebugObject(specialtyId);

            try
            {
                var IsUserLoggedIn = context.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.Unauthorized
                    };
                }

                var specialty = context.Specialties.Where(s => s.SpecialtyId == specialtyId).FirstOrDefault();
                if (specialty != null)
                {
                    specialty.LastEditedBy = IsUserLoggedIn.AdminId;
                    specialty.LastEditedDate = DateTime.Now;
                    specialty.IsEnabled = false;

                    context.Specialties.Update(specialty);
                    context.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = $"Successfully deleted {specialty.Name} Specialty.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Error Deleting Specialty Record",
                    Status = "Failed!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::SpecialtyRepository::DeleteSpecialty--");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }


            return apiResp;
        }

        private static APIResponse ResponseFailed(out APIResponse apiResp)
        {
            return apiResp = new APIResponse
            {
                Message = "Creation Failed. Duplicate Specialty found.",
                Status = "Failed!",
                StatusCode = HttpStatusCode.BadRequest
            };
        }
    }
}
