using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Enums
{
    public enum RequestStatus
    {
        New,
        Pending,
        Closed,
    }

    public enum RequestType
    {
        Report,
        UserRequest,
    }
}
