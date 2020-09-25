using System.Collections.Generic;
using Vescon.So.WebApi.Client.Dtos;

namespace Sample.EnvironmentInfos
{
    internal abstract class So3_DB_TestingExtern06 : EnvironmentInfoBase
    {
        public override string LayoutFacilityPath => "SO3/Projects/BMW/SL01/SL01/F95";
        public override string LayoutPageName => "Layout 01";

        public override string SymbolPath =>
            "SO3/Master Data/Symbol Libraries/Symbole/52 Roboter/ROB01";

        public override string SymbolPathWithConnectors =>
            "SO3/Master Data/Symbol Libraries/Symbole/23 PN Komponenten/PNP01";

        public override string MacroPath =>
            "SO3/Master Data/Macros/Schaltschränke/Mobile panel 1";

        public override string Identification => "==123456789=ABC++XX999";
        public override string IdentificationMainSymbolReference => "==XXX=ABC++XX999";
        public override string IdentificationConnector => "==XXX=ABC++XX999:X1:P1";
        public override string IdentificationConnectorNew => "==XXX=ABC++XX999:X42";

        public override List<AttributeValuePart> AttributeValuePartsIdentifying => new List<AttributeValuePart>
        {
            new AttributeValuePart("Werkekennung", "1234"),
            new AttributeValuePart("Einbauort (+)", "ABCD")
        };

        public override List<AttributeValuePart> AttributeValuePartsDescriptiveMultilanguage =>
            new List<AttributeValuePart>
            {
                new AttributeValuePart("Beschreibung", "en-US", "en value")
            };

        public override List<AttributeValuePart> AttributeValuePartsPropertyIndexed => new List<AttributeValuePart>
        {
            new AttributeValuePart("ArtNr", 42, "123456")
        };
    }
}