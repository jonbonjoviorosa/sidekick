using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.FireBase
{
    public class FCMNotificationModel
    {
        public FCMNotificationModel()
        {
            notification = new Notification();
            data = new Data();
        }
        public List<string> registration_ids { get; set; }
        public int mutable_a32_content { get; set; }
        public int time_to_live { get; set; }
        public string priority { get; set; }
        
        public Notification notification { get; set; }
        
        public Data data { get; set; }
    }

    public class Notification
    {
        public string body { get; set; }
        public string title { get; set; }
        public string priority { get; set; }

    }

    public class Data
    {
        public string body { get; set; }
        public string title { get; set; }
        public int notification_type { get; set; }
    }
}
