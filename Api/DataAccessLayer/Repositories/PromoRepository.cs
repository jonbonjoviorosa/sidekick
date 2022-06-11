using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class PromoRepository : APIBaseRepo, IPromoRepository
    {
        private readonly APIDBContext _dbContext;
        ILoggerManager _logManager { get; }
        public PromoRepository(APIDBContext dbContext,
            ILoggerManager logManager)
        {
            _dbContext = dbContext;
            _logManager = logManager;
        }

        /// <inheritdoc/>
        public async Task<APIResponse<IEnumerable<Promo>>> GetPromos()
        {
            var response = new APIResponse<IEnumerable<Promo>>();

            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var promos = await _dbContext.Promos.Where(p => p.IsEnabled == true).ToListAsync();
                return response = new APIResponse<IEnumerable<Promo>>
                {
                    Status = "Success!",
                    StatusCode = HttpStatusCode.OK,
                    Payload = promos
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                response.Message = "Something went wrong!";
                response.Status = "Internal Server Error";
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ModelError = GetStackError(ex.InnerException);
            }

            return response;
        }

        ///<inheritdoc/>
        public async Task<APIResponse> AddOrEditPromo(string _auth, Promo promo)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::PromoRepository::AddOrEditPromo --");
            _logManager.LogDebugObject(promo);

            try
            {
                var IsUserLoggedIn = _dbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.Unauthorized
                    };
                }

                var promos = await _dbContext.Promos.Where(p => p.IsEnabled == true).ToListAsync();
                var isPromoExisting = promos.Where(e => e.PromoId == promo.PromoId).FirstOrDefault();
                var GuidId = Guid.NewGuid();
                var TodaysDate = DateTime.Now;
                if (isPromoExisting == null)
                {
                    if (!promos.Where(e => e.Name == promo.Name && e.IsEnabled == true).Any())
                    {
                        promo.PromoId = GuidId;

                        promo.LastEditedBy = IsUserLoggedIn.AdminId;
                        promo.LastEditedDate = TodaysDate;
                        promo.CreatedBy = IsUserLoggedIn.AdminId;
                        promo.CreatedDate = TodaysDate;
                        promo.IsEnabled = true;
                        promo.IsEnabledBy = IsUserLoggedIn.AdminId;
                        promo.DateEnabled = TodaysDate;
                        promo.IsLocked = false;
                        promo.LockedDateTime = TodaysDate;


                        _dbContext.Promos.Add(promo);
                        await _dbContext.SaveChangesAsync();

                        return apiResp = new APIResponse
                        {
                            Message = "New Promo added successfully.",
                            Status = "Success!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate Promo name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
                else if (!string.IsNullOrWhiteSpace(isPromoExisting.PromoId.ToString()))
                {
                    if (promos.Where(g => g.Name == promo.Name && g.PromoId != promo.PromoId && g.IsEnabled == true).Any())
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Creation Failed. Duplicate Promo name found.",
                            Status = "Failed!",
                            StatusCode = HttpStatusCode.BadRequest
                        };
                    }

                    isPromoExisting.LastEditedBy = IsUserLoggedIn.AdminId;
                    isPromoExisting.LastEditedDate = TodaysDate;
                    isPromoExisting.CreatedBy = IsUserLoggedIn.AdminId;
                    isPromoExisting.CreatedDate = TodaysDate;
                    isPromoExisting.IsEnabled = true;
                    isPromoExisting.IsEnabledBy = IsUserLoggedIn.AdminId;
                    isPromoExisting.DateEnabled = TodaysDate;
                    isPromoExisting.IsLocked = false;
                    isPromoExisting.LockedDateTime = TodaysDate;
                    isPromoExisting.Name = promo.Name;
                    isPromoExisting.PromoType = promo.PromoType;
                    isPromoExisting.IsActive = promo.IsActive;
                    isPromoExisting.Amount = promo.Amount;
                    isPromoExisting.Code = promo.Code;
                    isPromoExisting.StartsFrom = promo.StartsFrom;
                    isPromoExisting.ValidTo = promo.ValidTo;
                    isPromoExisting.CoachId = promo.CoachId;
                    isPromoExisting.ByFacility = promo.ByFacility;
                    isPromoExisting.FacilityId = promo.FacilityId;
                    isPromoExisting.EventType = promo.EventType;
                    isPromoExisting.AllCoaches = promo.AllCoaches;

                    _dbContext.Promos.Update(isPromoExisting);
                    await _dbContext.SaveChangesAsync();

                    return apiResp = new APIResponse
                    {
                        Message = "Updated Promo successfully.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate Promo name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::PromoRepository::AddOrEditPromo--");
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        ///<inheritdoc/>
        public  APIResponse GetPromoById(Guid promoId)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::PromoRepository::GetPromoById --");
            _logManager.LogDebugObject(promoId);

            try
            {
                var promo =  _dbContext.Promos.Where(p => p.PromoId == promoId).FirstOrDefault();
                if(promo != null)
                {
                    return apiResp = new APIResponse
                    {
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK,
                        Payload = promo
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Promo not existing.",
                    Status = "Failed!",
                    StatusCode = HttpStatusCode.Unauthorized
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::PromoRepository::GetPromoById--");
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        ///<inheritdoc/>
        public APIResponse DeletePromo(string _auth, Guid promoId)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::PromoRepository::DeletePromo --");
            _logManager.LogDebugObject(promoId);

            try
            {
                var IsUserLoggedIn = _dbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.Unauthorized
                    };
                }

                var promo = _dbContext.Promos.Where(s => s.PromoId == promoId).FirstOrDefault();
                if (promo != null)
                {
                    promo.LastEditedBy = IsUserLoggedIn.AdminId;
                    promo.LastEditedDate = DateTime.Now;
                    promo.IsEnabled = false;

                    _dbContext.Promos.Update(promo);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = $"Successfully deleted {promo.Name} Promo.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Error Deleting Promo Record",
                    Status = "Failed!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::PromoRepository::DeletePromo--");
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }
    }
}
