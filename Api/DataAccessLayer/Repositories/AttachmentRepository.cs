
using System;
using System.Linq;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System.IO;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class AttachmentRepository : APIBaseRepo, IAttachmentRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public AttachmentRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse AddAttachmentRecord(string _authToken, string _uploadedFileName, string _serverPhyPath, string _urlRoot, UploadTypes _ut)
        {
            string fileFolder = "";
            switch (_ut)
            {
                case UploadTypes.UploadBanner:
                    fileFolder = "Banners";
                    break;
                case UploadTypes.UploadIcon:
                    fileFolder = "Icons";
                    break;
                case UploadTypes.UploadBackgroundImage:
                    fileFolder = "BackgroundImages";
                    break;
                case UploadTypes.UploadProfileImage:
                    fileFolder = "ProfileImages";
                    break;
                case UploadTypes.UploadDocument:
                    fileFolder = "Documents";
                    break;
                default:
                    fileFolder = "";
                    break;
            }

            APIResponse ApiResp = new APIResponse();
            try
            {
                UserLoginTransaction ULT = DbContext.UserLoginTransactions.Where(ult => ult.Token == _authToken && ult.IsEnabled == true).FirstOrDefault();
                if (ULT != null)
                {
                    Attachment newAttachment = SaveAttachment(ULT.UserId, fileFolder, _uploadedFileName, _serverPhyPath, _urlRoot, _ut);

                    return new APIResponse
                    {
                        Message = "Record Added!",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = newAttachment
                    };
                }

                FacilityUserLoginTransaction FULT = DbContext.FacilityUserLoginTransactions.Where(ult => ult.Token == _authToken && ult.IsEnabled == true).FirstOrDefault();
                if (FULT != null)
                {
                    Attachment newAttachment = SaveAttachment(FULT.FacilityUserId, fileFolder, _uploadedFileName, _serverPhyPath, _urlRoot, _ut);

                    return new APIResponse
                    {
                        Message = "Record Added!",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = newAttachment
                    };
                }

                AdminLoginTransaction AULT = DbContext.AdminLoginTransactions.Where(ult => ult.Token == _authToken && ult.IsEnabled == true).FirstOrDefault();
                if (AULT != null)
                {
                    Attachment newAttachment = SaveAttachment(AULT.AdminId, fileFolder, _uploadedFileName, _serverPhyPath, _urlRoot, _ut);

                    return new APIResponse
                    {
                        Message = "Record Added!",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = newAttachment
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddAttachmentRecord");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                ApiResp.Message = "Something Went wrong!";
                ApiResp.Status = "Internal Server Error";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }
            return ApiResp;
        }

        public Attachment SaveAttachment(Guid UserId, string fileFolder, string _uploadedFileName, string _serverPhyPath, string _urlRoot, UploadTypes _ut)
        {
            string ActualFileName = GetNewFileName(_uploadedFileName, UserId.ToString().Split('-')[4]);

            DateTime CreatedNow = DateTime.Now;
            Attachment NewAttach = new Attachment
            {
                AttachmentExternalUrl = _urlRoot + "/resources/" + fileFolder + "/" + ActualFileName,
                AttachmentName = ActualFileName,
                AttachmentFileName = Path.GetFileNameWithoutExtension(ActualFileName),
                AttachmentServerPhysicalPath = Path.Combine(_serverPhyPath, ActualFileName),
                AttachmentType = Path.GetExtension(_uploadedFileName),
                AttachmentUploadedFileName = _uploadedFileName,
                CreatedBy = UserId,
                CreatedDate = CreatedNow,
                UType = _ut,
                DateEnabled = CreatedNow,
                IsEnabled = true,
                IsEnabledBy = UserId,
                LastEditedBy = UserId,
                LastEditedDate = CreatedNow
            };

            DbContext.Add(NewAttach);
            DbContext.SaveChanges();

            LogManager.LogInfo("-- Saved New File --");
            LogManager.LogInfo("Path: " + NewAttach.AttachmentServerPhysicalPath);
            LogManager.LogDebugObject(NewAttach);

            return NewAttach;
        }

        string GetNewFileName(string _upFName, string _uidPrefx)
        {
            return _uidPrefx + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + Path.GetExtension(_upFName);
        }
    }
}
