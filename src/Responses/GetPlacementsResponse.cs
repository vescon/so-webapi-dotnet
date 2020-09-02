using System.Collections.Generic;
using Sample.Dtos;

namespace Sample.Responses
{
    public class GetPlacementsResponse
    {
        public List<Placement> Placements { get; set; }

        public bool HasNext { get; set; }
    }
}