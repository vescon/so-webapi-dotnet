using System.Collections.Generic;
using Sample.Dtos;

namespace Sample.SampleInfos
{
    internal abstract class So3_DB_TestingExtern06 : SampleInfo
    {
        public override string LayoutFacilityPath => "SO3/Projects/VESCON/Sample/Sample/Sample/Facility1";

        public override string SymbolPath =>
            "SO3/Master Data/Symbol Libraries/Symbole/52 Roboter/ROB01";

        public override string Identification => "==123456789=ABC++XX999";

        public override List<AttributeValuePart> AttributeValuePartsIdentifying => new List<AttributeValuePart>
        {
            new AttributeValuePart("Werkekennung", "1234"),
            new AttributeValuePart("Einbauort (+)", "ABCD")
        };
    }
}