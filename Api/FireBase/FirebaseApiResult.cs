using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.FireBase
{
    public class FirebaseApiResult
    {
        public object multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public List<FirebaseResult> results { get; set; }
    }

    public class FirebaseResult
    {
        public string error { get; set; }
        public string message_id { get; set; }
    }
}
