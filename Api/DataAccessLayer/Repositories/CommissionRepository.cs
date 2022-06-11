using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class CommissionRepository : APIBaseRepo, ICommissionRepository
    {
        private readonly APIDBContext _dbContext;
        ILoggerManager _logManager { get; }
        public CommissionRepository(APIDBContext dbContext,
            ILoggerManager logManager)
        {
            _dbContext = dbContext;
            _logManager = logManager;
        }

        /// <inheritdoc/>
        public async Task<APIResponse<IEnumerable<CommissionPlay>>> ComissionPlays()
        {
            var response = new APIResponse<IEnumerable<CommissionPlay>>();

            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var comissionPlays = await _dbContext.ComissionPlays.ToListAsync();
                return response = new APIResponse<IEnumerable<CommissionPlay>>
                {
                    Status = "Success!",
                    StatusCode = HttpStatusCode.OK,
                    Payload = comissionPlays
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

        /// <inheritdoc/>
        public async Task<APIResponse<CommissionTrain>> ComissionTrains()
        {
            var response = new APIResponse<CommissionTrain>();

            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var comissionTrains = await _dbContext.ComissionTrains.FirstOrDefaultAsync();
                return response = new APIResponse<CommissionTrain>
                {
                    Status = "Success!",
                    StatusCode = HttpStatusCode.OK,
                    Payload = comissionTrains
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
        public async Task<APIResponse> AddOrEditComissionPlay(string _auth, List<CommissionPlaySportViewModel> plays)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::ComissionRepository::AddOrEditComissionPlay --");
            _logManager.LogDebugObject(plays);

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

                var existingPlays = await _dbContext.ComissionPlays.ToListAsync();
                if (existingPlays.Any())
                {
                    _dbContext.ComissionPlays.RemoveRange(existingPlays);
                }

                var comissionPlays = new List<CommissionPlay>();
                foreach (var item in plays)
                {
                    var newComission = new CommissionPlay
                    {
                        SportId = item.SportId,
                        ComissionPerPlayer = item.ComissionPerPlayer,

                        LastEditedBy = IsUserLoggedIn.AdminId,
                        LastEditedDate = DateTime.Now,
                        CreatedBy = IsUserLoggedIn.AdminId,
                        CreatedDate = DateTime.Now,
                        IsEnabled = true,
                        IsEnabledBy = IsUserLoggedIn.AdminId,
                        DateEnabled = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now
                    };

                    comissionPlays.Add(newComission);
                }

                _dbContext.ComissionPlays.AddRange(comissionPlays);
                await _dbContext.SaveChangesAsync();

                return apiResp = new APIResponse
                {
                    Status = "Success!",
                    StatusCode = HttpStatusCode.OK,
                    Message = "Successfully Added Comission for Set Up Play"
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Run::ComissionRepository::AddOrEditComissionPlay --");
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
        public APIResponse AddOrEditComissionTrain(string _auth, CommissionTrain train)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::ComissionRepository::AddOrEditComissionTrain --");
            _logManager.LogDebugObject(train);

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

                var existingTrain = _dbContext.ComissionTrains.FirstOrDefault();
                if (existingTrain != null)
                {
                    existingTrain.CoachingGroupComission = train.CoachingGroupComission;
                    existingTrain.CoachingIndividualComission = train.CoachingIndividualComission;

                    existingTrain.LastEditedBy = IsUserLoggedIn.AdminId;
                    existingTrain.LastEditedDate = DateTime.Now;

                    _dbContext.ComissionTrains.Update(existingTrain);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK,
                        Message = "Successfully updated Comission for Set Up Train"
                    };
                }


                train.LastEditedBy = IsUserLoggedIn.AdminId;
                train.LastEditedDate = DateTime.Now;
                train.CreatedBy = IsUserLoggedIn.AdminId;
                train.CreatedDate = DateTime.Now;
                train.IsEnabled = true;
                train.IsEnabledBy = IsUserLoggedIn.AdminId;
                train.DateEnabled = DateTime.Now;
                train.IsLocked = false;
                train.LockedDateTime = DateTime.Now;

                _dbContext.ComissionTrains.Add(train);
                _dbContext.SaveChanges();

                return apiResp = new APIResponse
                {
                    Status = "Success!",
                    StatusCode = HttpStatusCode.OK,
                    Message = "Successfully Added Comission for Set Up Train"
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Run::ComissionRepository::AddOrEditComissionTrain --");
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse<IEnumerable<CommisionReport>>> GetComissionReport()
        {
            var response = new APIResponse<IEnumerable<CommisionReport>>();
            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);

            try
            {
                var reports = new List<CommisionReport>();
                var query = from x in _dbContext.IndividualBookings
                            join y in _dbContext.IndividualClasses
                                on x.ClassId equals y.ClassId
                            join z in _dbContext.Users
                                on y.CoachId equals z.UserId
                            select new CommisionReport
                            {
                                FirstName = z.FirstName,
                                LastName = z.LastName,
                                TotalSalesAmount = (double)x.TotalAmount,
                                VatAmount = (double)x.ServiceFees,
                                CommissionAmount = (double)x.SideKickCommission,
                                BookingType = EBookingType.Individual
                            };

                var queryGroup = from x in _dbContext.GroupBookings
                                 join y in _dbContext.GroupClasses
                                     on x.GroupClassId equals y.GroupClassId
                                 join z in _dbContext.Users
                                     on y.CoachId equals z.UserId
                                 select new CommisionReport
                                 {
                                     FirstName = z.FirstName,
                                     LastName = z.LastName,
                                     TotalSalesAmount = (double)x.TotalAmount,
                                     VatAmount = (double)x.ServiceFees,
                                     CommissionAmount = (double)x.SideKickCommission,
                                     BookingType = EBookingType.Group
                                 };

                var queryPlay = from x in _dbContext.FacilityPlayers
                                join z in _dbContext.Users
                                    on x.UserId equals z.UserId
                                join up in _dbContext.UserPitchBookings
                                    on x.BookingId equals up.BookingId
                                select new CommisionReport
                                {
                                    FirstName = z.FirstName,
                                    LastName = z.LastName,
                                    TotalSalesAmount = (double)x.TotalAmount,
                                    VatAmount = (double)up.PricePerUserVat,
                                    CommissionAmount = (double)up.Commission,
                                    BookingType = EBookingType.Play
                                };

                reports = await query.ToListAsync();
                var groupbooking = await queryGroup.ToListAsync();
                reports.AddRange(groupbooking);

                return response = new APIResponse<IEnumerable<CommisionReport>>
                {
                    Status = "Success!",
                    StatusCode = HttpStatusCode.OK,
                    Payload = reports
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
    }
}
