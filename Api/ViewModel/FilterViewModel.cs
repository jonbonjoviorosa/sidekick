using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.ViewModel
{
    public class FilterViewModel
    {
        public FilterType FilterType { get; set; }
        public string FilterValue { get; set; }
    }
}
