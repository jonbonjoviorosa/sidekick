using Sidekick.Model;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IAttachmentRepository
    {
        APIResponse AddAttachmentRecord(string _authToken, string _uploadedFileName, string _serverPhyPath, string _urlRoot, UploadTypes _ut);
    }
}
