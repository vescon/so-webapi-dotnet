using System.Collections.Generic;
using Sample.Dtos;

namespace Sample.Responses
{
    public class CreatePlacementResponse
    {
        public List<PlacementHeader>? Placements { get; set; }
    }
}
