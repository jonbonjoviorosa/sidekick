using Sidekick.Model;

namespace Sidekick.Api.ViewModel
{
    public class PagedRequestViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SortColumn { get; set; }
        public bool IsAscending { get; set; }
    }
}
