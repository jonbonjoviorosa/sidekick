using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Gym
{
    public class GymViewModel
    {
        public Guid GymId { get; set; }
        public string Gym { get; set; }
        public string Image { get; set; }
    }

    public class GymLocationViewModel
    {
        public Guid GymId { get; set; }
        public string Gym { get; set; }
        public string Image { get; set; }
        public string GymAddress { get; set; }
        public float? GymLat { get; set; }
        public float? GymLong { get; set; }
    }

}
