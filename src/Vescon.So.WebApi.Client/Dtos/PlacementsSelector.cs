using System;

namespace Vescon.So.WebApi.Client.Dtos
{
    public class PlacementsSelector
    {
        public PlacementsSelector()
        {
        }

        public PlacementsSelector(Guid placementGuid)
        {
            PlacementGuid = placementGuid;
        }

        public PlacementsSelector(string identificationPrefix)
        {
            IdentificationPrefix = identificationPrefix;
        }

        public Guid? PlacementGuid { get; }
        public string? IdentificationPrefix { get; }
    }
}