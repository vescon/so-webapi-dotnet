using System.Collections.Generic;
using Sample.Dtos;

namespace Sample.EnvironmentInfos
{
    internal abstract class So3_DB_WebApiSource : EnvironmentInfoBase
    {
        public override string LayoutFacilityPath => "SO3/Projects/Sample/Sample/Sample/Sample Facility";

        public override string SymbolPath =>
            "SO3/Master Data/Symbol Libraries/Symbols - Symbole/17 Robots - Roboter/ROB01";

        public override string MacroPath =>
            "SO3/Master Data/Macros/1Robot_1SymbolWith4Connectors_3Regions";

        public override string Identification => "==123=ABC+456";

        public override List<AttributeValuePart> AttributeValuePartsIdentifying => new List<AttributeValuePart>
        {
            new AttributeValuePart("Plant", "XXX"),
            new AttributeValuePart("Product", "PPP")
        };

        public override List<AttributeValuePart> AttributeValuePartsDescriptiveMultilanguage =>
            new List<AttributeValuePart>
            {
                new AttributeValuePart("Descriptive multilanguage", "en-US", "en value"),
                new AttributeValuePart("Descriptive multilanguage", "de-DE", "de value")
            };

        public override List<AttributeValuePart> AttributeValuePartsPropertyIndexed => new List<AttributeValuePart>
        {
            new AttributeValuePart("Property Default", "value"),
            new AttributeValuePart("Property Multilanguage", "de-DE", "de value"),
            new AttributeValuePart("Property Multilanguage", "en-US", "en value"),
            new AttributeValuePart("Property Indexed", 42, "indexed value")
        };
    }
}