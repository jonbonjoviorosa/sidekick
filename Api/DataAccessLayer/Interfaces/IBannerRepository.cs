using System;
using System.Threading.Tasks;
using Sidekick.Model;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IBannerRepository
    {
        APIResponse Add(string _auth, BannerDto _banner);
        APIResponse Edit(string _auth, BannerDto _banner);
        APIResponse List();
        APIResponse GetBanner(Guid _bannerID);
        APIResponse FacilityBanner(Guid _facilityID);
        Task<APIResponse> Delete(Guid _bannerID);
    }
}
