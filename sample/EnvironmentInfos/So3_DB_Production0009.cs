using System.Collections.Generic;
using Vescon.So.WebApi.Client.Dtos;

namespace Sample.EnvironmentInfos
{
    internal abstract class So3_DB_Production0009 : EnvironmentInfoBase
    {
        public override string LayoutFacilityPath => "SO3/Projects/Z_Vescon/Hegh_01/Hegh_01/RK";
        public override string LayoutPageName => "Layout 01";

        public override string SymbolPath =>
            "SO3/Master Data/Symbol Libraries/Symbols - Symbole/17 Robots - Roboter/ROB01";

        public override string SymbolPathWithConnectors =>
            "SO3/Master Data/Symbol Libraries/Symbols - Symbole/11 PN components - PN Komponenten/PBL01";

        public override string MacroPath =>
            "SO3/Master Data/Macros/RC_001";

        public override string Identification => "==112234445=XXX666+7-8";
        public override string IdentificationMainSymbolReference => "==112234445=ABC666+7-8";
        public override string IdentificationConnector => IdentificationMainSymbolReference + ":P1R";
        public override string IdentificationConnectorNew => IdentificationMainSymbolReference + ":X42";

        public override List<AttributeValuePart> AttributeValuePartsIdentifying => new List<AttributeValuePart>
        {
            new AttributeValuePart("Anlagenkennzeichnung", "YYY999"),
            new AttributeValuePart("Einbauort", "ABCD")
        };

        public override List<AttributeValuePart> AttributeValuePartsDescriptiveMultilanguage => null;

        public override List<AttributeValuePart> AttributeValuePartsPropertyIndexed => new List<AttributeValuePart>
        {
            new AttributeValuePart("Order code", 42, "123456")
        };
    }
}