using System;

namespace Sample.Dtos
{
    /// <summary>
    /// Only one of the options is set.
    /// </summary>
    public class PlacementsSelector
    {
        public PlacementsSelector(bool all)
        {
            All = all;
        }

        public PlacementsSelector(string identificationPrefix)
        {
            IdentificationPrefix = identificationPrefix;
        }
        
        public PlacementsSelector(Guid placementGuid)
        {
            PlacementGuid = placementGuid;
        }

        public bool? All { get; }
        public string? IdentificationPrefix { get; }
        
        public Guid? PlacementGuid { get; set; }
    }
}