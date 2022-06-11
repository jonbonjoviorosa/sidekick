using Sidekick.Model.Enums;
using System;

namespace Sidekick.Model
{
    public class UserReview : APIBaseModel
    {
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public Ratings Ratings { get; set; }
        public Guid ClassId { get; set; }
        public Guid CoachId { get; set; }
        public string Coach { get; set; }
        public ReviewType Type { get; set; }
    }

    public class UserReviews
    {
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public Ratings Ratings { get; set; }
        public Guid ClassId { get; set; }
        public Guid CoachId { get; set; }
        public string Coach { get; set; }
        public string CoachImage { get; set; }
        public ReviewType Type { get; set; }
    }
}