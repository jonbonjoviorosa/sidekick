using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System.Collections.Generic;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class FAQsRepository : APIBaseRepo, IFAQsRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public FAQsRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse Add(string _auth, FAQsDto _fAQ)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FAQsRepository::Add --");
            LogManager.LogDebugObject(_fAQ);

            try
            {
                AdminLoginTransaction IsUserLoggedIn = DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                FAQs hasDuplicate = DbContext.FAQs.AsNoTracking().FirstOrDefault(x => x.Question == _fAQ.Question && x.IsEnabled == true);
                if (hasDuplicate == null)
                {
                    DateTime TodaysDate = DateTime.Now;
                    Guid GuidId = Guid.NewGuid();

                    FAQs fAQs = new()
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

                        FAQsId = GuidId,
                        Question = _fAQ.Question,
                        Answer = _fAQ.Answer
                    };

                    DbContext.FAQs.Add(fAQs);
                    DbContext.SaveChanges();

                    apiResp = new APIResponse
                    {
                        Message = "New FAQ added successfully.",
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
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FAQsRepository::Add --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse Edit(string _auth, FAQsDto _fAQ)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FAQsRepository::Edit --");
            LogManager.LogDebugObject(_fAQ);

            try
            {
                AdminLoginTransaction IsUserLoggedIn = DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                FAQs foundRecord = DbContext.FAQs.AsNoTracking().FirstOrDefault(x => x.FAQsId == _fAQ.FAQsId);
                if (foundRecord != null)
                {
                    FAQs hasDuplicate = DbContext.FAQs.AsNoTracking().FirstOrDefault(x => x.FAQsId != _fAQ.FAQsId && x.Question == _fAQ.Question);
                    if (hasDuplicate == null)
                    {
                        DateTime TodaysDate = DateTime.Now;

                        foundRecord.LastEditedBy = IsUserLoggedIn.AdminId;
                        foundRecord.LastEditedDate = TodaysDate;

                        foundRecord.Question = _fAQ.Question;
                        foundRecord.Answer = _fAQ.Answer;

                        DbContext.FAQs.Update(foundRecord);
                        DbContext.SaveChanges();

                        apiResp = new APIResponse
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
                            Message = "Processing Failed. Existing found.",
                            Status = "Failed!",
                            StatusCode = System.Net.HttpStatusCode.Found
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
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FAQsRepository::Edit --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse List(Guid? _fAQsId = null)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::FAQsRepository::List --" + _fAQsId);

            try
            {
                IEnumerable<FAQsDto> fAQsList = null;
                fAQsList = DbContext.FAQs.AsNoTracking().Where(w => w.IsEnabled == true)
                                     .Select(i => new FAQsDto
                                     {
                                         FAQsId = i.FAQsId,
                                         Question = i.Question,
                                         Answer = i.Answer
                                     }).ToList();

                if (fAQsList.Any())
                {
                    if (_fAQsId == null)
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "All records found.",
                            Status = "Success!",
                            Payload = fAQsList,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "All records found.",
                            Status = "Success!",
                            Payload = fAQsList.Where(x => x.FAQsId == _fAQsId).FirstOrDefault(),
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "No records found.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FAQsRepository::List --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse ViewLegalDoc(ELegalDocType _type)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::FAQsRepository::ViewLegalDoc --");

            try
            {
                LegalDocument legalDocument = DbContext.LegalDocuments.AsNoTracking().Where(x => x.DocType == _type).FirstOrDefault();
                if (legalDocument != null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "All records found.",
                        Status = "Success!",
                        Payload = legalDocument,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "No records found.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FAQsRepository::ViewLegalDoc --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public APIResponse Status(string _auth, FAQStatus _fAQ)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::ChangeFAQStatus --");
            LogManager.LogDebugObject(_fAQ);

            try
            {
                AdminLoginTransaction IsUserLoggedIn = DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized Access",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                FAQs foundRecord = DbContext.FAQs.AsNoTracking().FirstOrDefault(x => x.FAQsId == _fAQ.Id);
                if (foundRecord != null)
                {
                    DateTime TodaysDate = DateTime.Now;
                    foundRecord.LastEditedBy = IsUserLoggedIn.AdminId;
                    foundRecord.LastEditedDate = TodaysDate;
                    foundRecord.IsEnabled = _fAQ.IsEnabled;

                    DbContext.FAQs.Update(foundRecord);
                    DbContext.SaveChanges();

                    var returnMsg = _fAQ.IsEnabled == false ? "Deleted Successfully" : "Activated Successfully";
                    apiResp = new APIResponse
                    {
                        Message = returnMsg,
                        Status = "Success",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "No Records Found",
                        Status ="Failed",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::ChangeFAQStatus --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse AddEditLegalDoc(string _auth, LegalDocumentDto _legal)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::FAQsRepository::EditLegaDoc --");
            LogManager.LogDebugObject(_legal);

            try
            {
                AdminLoginTransaction IsUserLoggedIn = DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized Access",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                DateTime TodaysDate = DateTime.Now;
                LegalDocument foundRecord = DbContext.LegalDocuments.AsNoTracking().Where(x => x.DocType == _legal.DocType).FirstOrDefault();
                if (foundRecord == null) //add new
                {
                    LegalDocument legalDocument = new()
                    {
                        Body = _legal.Body,
                        DocType = _legal.DocType,
                        LastEditedBy = IsUserLoggedIn.AdminId,
                        LastEditedDate = TodaysDate
                    };

                    DbContext.LegalDocuments.Add(legalDocument);
                    DbContext.SaveChanges();

                    apiResp = new APIResponse
                    {
                        Message = "Added Successfully",
                        Status = "Success",
                        Payload = legalDocument,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    foundRecord.Body = _legal.Body;
                    foundRecord.DocType = _legal.DocType;
                    foundRecord.LastEditedBy = IsUserLoggedIn.AdminId;
                    foundRecord.LastEditedDate = TodaysDate;

                    DbContext.LegalDocuments.Update(foundRecord);
                    DbContext.SaveChanges();

                    return apiResp = new APIResponse
                    { 
                        Message = "Updated Successfully",
                        Status = "Success",
                        Payload = foundRecord,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FAQsRepository::EditLegaDoc --");
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
