using Microsoft.EntityFrameworkCore;
using Sidekick.Api.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Helpers
{
    public class Pagination<T> where T : class
    {
        public static async Task Data(int pageSize, 
            int pageIndex,
            PagedViewModel<T> list, IQueryable<T> data)
        {
            list.Data = await data
                            .Skip(pageSize * (pageIndex - 1))
                            .Take(pageSize)
                            .ToListAsync();

            list.CurrentPage = pageIndex;
            list.RecordCount = await data.CountAsync();
            list.NumberofPages = Convert.ToInt32(Math.Ceiling((double)list.RecordCount / pageSize));
            list.PageSize = pageSize;
        }
    }
}
