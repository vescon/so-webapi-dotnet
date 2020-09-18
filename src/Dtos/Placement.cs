using System;
using System.Collections.Generic;

namespace Sample.Dtos
{
    public class Placement
    {
        public Guid Guid { get; set; }
        public string Type { get; set; }
        public string? Identification { get; set; }
        public string? TypePath { get; set; }
        public Point Location { get; set; }
        public float RotationZ { get; set; }
        
        public List<AttributeValuePart> AttributeValueParts { get; set; }
    }
}