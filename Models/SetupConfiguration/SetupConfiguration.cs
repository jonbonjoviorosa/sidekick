using Sidekick.Model.SetupConfiguration.Goals;
using Sidekick.Model.SetupConfiguration.Size;
using System.Collections.Generic;

namespace Sidekick.Model.SetupConfiguration
{
    public class SetupConfiguration
    {
        public IEnumerable<Surface> Surfaces { get; set; }
        public IEnumerable<Location> Locations { get; set; }
        public IEnumerable<TeamSize> TeamSize { get; set; }
        public IEnumerable<Sport> Sports { get; set; }
        public IEnumerable<Specialty.Specialty> Specialties { get; set; }
        public IEnumerable<Goal> Goals { get; set; }
        public IEnumerable<Gym.Gym> Gyms { get; set; }
        public IEnumerable<Level.Level> Levels { get; set; }
        public IEnumerable<FacilityUserType> FacilityUserTypes { get; set; }
    }
}
