using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Goals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class GoalRepository : APIBaseRepo, IGoalRepository
    {
        private readonly APIDBContext _dbContext;
        ILoggerManager _logManager { get; }
        public GoalRepository(APIDBContext dbContext,
            ILoggerManager logManager)
        {
            _dbContext = dbContext;
            _logManager = logManager;
        }

        /// <inheritdoc/>
        public async Task<APIResponse<IEnumerable<Goal>>> GetGoals()
        {
            var response = new APIResponse<IEnumerable<Goal>>();

            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var goals = await _dbContext.Goals.Where(g => g.IsEnabled == true).ToListAsync();
                return response = new APIResponse<IEnumerable<Goal>>
                {
                    Status = "Success!",
                    StatusCode = HttpStatusCode.OK,
                    Payload = goals
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
        public APIResponse AddOrEditGoal(string _auth, Goal goal)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::GoalRepository::AddOrEditGoal --");
            _logManager.LogDebugObject(goal);

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

                var goals = _dbContext.Goals;
                var isGoalExisting = goals.Where(e => e.GoalId == goal.GoalId).FirstOrDefault();
                var GuidId = Guid.NewGuid();
                var TodaysDate = DateTime.Now;
                if (isGoalExisting == null)
                {
                    if (!goals.Where(e => e.Name == goal.Name).Any())
                    {
                        var newGoal = new Goal
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
                            GoalId = GuidId,
                            Name = goal.Name,
                        };

                        _dbContext.Goals.Add(newGoal);
                        _dbContext.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "New Goal added successfully.",
                            Status = "Success!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate Goal name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
                else if (!string.IsNullOrWhiteSpace(isGoalExisting.GoalId.ToString()))
                {
                    if(goals.Where(g => g.Name == goal.Name).Any())
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Creation Failed. Duplicate Goal name found.",
                            Status = "Failed!",
                            StatusCode = HttpStatusCode.BadRequest
                        };
                    }
                    isGoalExisting.LastEditedBy = IsUserLoggedIn.AdminId;
                    isGoalExisting.LastEditedDate = TodaysDate;
                    isGoalExisting.CreatedBy = IsUserLoggedIn.AdminId;
                    isGoalExisting.CreatedDate = TodaysDate;
                    isGoalExisting.IsEnabled = true;
                    isGoalExisting.IsEnabledBy = IsUserLoggedIn.AdminId;
                    isGoalExisting.DateEnabled = TodaysDate;
                    isGoalExisting.IsLocked = false;
                    isGoalExisting.LockedDateTime = TodaysDate;
                    isGoalExisting.Name = goal.Name;


                    _dbContext.Goals.Update(isGoalExisting);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Updated Goal successfully.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate Goal name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::GoalRepository::AddOrEditGoal--");
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
        public APIResponse DeleteGoal(string _auth, Guid goalId)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::GoalRepository::DeleteGoal --");
            _logManager.LogDebugObject(goalId);

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

                var goal = _dbContext.Goals.Where(s => s.GoalId == goalId).FirstOrDefault();
                if (goal != null)
                {
                    goal.LastEditedBy = IsUserLoggedIn.AdminId;
                    goal.LastEditedDate = DateTime.Now;
                    goal.IsEnabled = false;

                    _dbContext.Goals.Update(goal);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = $"Successfully deleted {goal.Name} Goal.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Error Deleting Goal Record",
                    Status = "Failed!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::GoalRepository::DeleteGoal--");
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
