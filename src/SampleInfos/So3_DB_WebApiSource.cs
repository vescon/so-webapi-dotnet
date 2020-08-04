using System.Collections.Generic;
using Sample.Dtos;

namespace Sample.SampleInfos
{
    internal abstract class So3_DB_WebApiSource : SampleInfo
    {
        public override string LayoutFacilityPath => "SO3/Projects/Sample/Sample/Sample/Sample Facility";

        public override string SymbolPath =>
            "SO3/Master Data/Symbol Libraries/Symbols - Symbole/17 Robots - Roboter/ROB01";

        public override string Identification => "==123=ABC+456";

        public override List<AttributeValuePart> AttributeValuePartsIdentifying => new List<AttributeValuePart>
        {
            new AttributeValuePart("Plant", "XXX"),
            new AttributeValuePart("Product", "PPP"),
        };

        public override List<AttributeValuePart> AttributeValuePartsDescriptiveMultilanguage => new List<AttributeValuePart>
        {
            new AttributeValuePart("Desc Multilingual", "de-DE", "de value")
        };
    }
}