using System;

namespace Sidekick.Model
{
    public class BlockViewModel
    {
        public Guid BlockedByUserId { get; set; }
        public string BlockedBy { get; set; }
        public Guid BlockedUserId { get; set; }
        public string BlockedUser { get; set; }
        public string ImageUrl { get; set; }
    }
}
