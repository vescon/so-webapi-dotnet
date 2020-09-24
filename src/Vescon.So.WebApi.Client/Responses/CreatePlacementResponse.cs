using System.Collections.Generic;
using Vescon.So.WebApi.Client.Dtos;

namespace Vescon.So.WebApi.Client.Responses
{
    public class CreatePlacementResponse
    {
        public List<PlacementHeader>? Placements { get; set; }
    }
}
