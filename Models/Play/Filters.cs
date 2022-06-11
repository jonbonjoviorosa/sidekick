using Sidekick.Model.SetupConfiguration.Level;
using Sidekick.Model.SetupConfiguration.Size;
using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class Filters
    {
        public IEnumerable<Area> AreaFilters { get; set; }
        public IEnumerable<Location> LocationFilters { get; set; }
        public IEnumerable<Surface> SurfaceFilters { get; set; }
        public IEnumerable<TeamSize> TeamSizeFilters { get; set; }
        public IEnumerable<Sport> SportFilters { get; set; }
        public IEnumerable<Gym.Gym> GymFilters { get; set; }
        public IEnumerable<Language.Language> LanguageFilters { get; set; }
        public IEnumerable<string> Prices { get; set; }

        public IEnumerable<Level> LevelFilters { get; set; }
    }
}
