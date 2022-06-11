using System;

namespace Sidekick.Model
{
    public class CommissionPlay : APIBaseModel
    {
        public Guid SportId { get; set; }
        public decimal ComissionPerPlayer { get; set; }
    }
}
