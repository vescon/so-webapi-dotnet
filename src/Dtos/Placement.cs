using System.Collections.Generic;
using System.Drawing;
using Sample.Dtos;

namespace WebApi.Api.V1.Layouts
{
    public class Placement
    {
        public PlacementsHeader Header { get; set; }
        public Point Location { get; set; }
        public float RotationZ { get; set; }
        
        public List<AttributeValuePart> AttributeValueParts { get; set; }
    }
}