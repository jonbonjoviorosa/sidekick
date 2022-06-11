using Sidekick.Model.Enums;
using System;

namespace Sidekick.Model
{
    public  class Request : APIBaseModel
    { 
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public RequestStatus Status { get; set; }
    }
}
