using System.Collections.Generic;
using Vescon.So.WebApi.Client.Dtos;

namespace Vescon.So.WebApi.Client.Responses
{
    public class GetPlacementsResponse
    {
        public List<Placement> Placements { get; set; }

        public bool HasNext { get; set; }
    }
}