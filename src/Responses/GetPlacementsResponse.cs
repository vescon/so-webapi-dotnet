using System.Collections.Generic;

namespace WebApi.Api.V1.Layouts
{
    public class GetPlacementsResponse
    {
        public List<Placement> Placements { get; set; }

        public bool HasNext { get; set; }
    }
}