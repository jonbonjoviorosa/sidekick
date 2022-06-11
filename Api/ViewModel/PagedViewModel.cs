using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.ViewModel
{
    public class PagedViewModel<T> where T : class
    {
        public PagedViewModel()
        {
            PageSize = 10;
        }
        public IEnumerable<T> Data { get; set; }
        public int NumberofPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int RecordCount { get; set; }
    }
}
