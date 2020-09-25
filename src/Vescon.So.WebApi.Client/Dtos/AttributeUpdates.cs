using System.Collections.Generic;

namespace Vescon.So.WebApi.Client.Dtos
{
    public class AttributeUpdates
    {
        public AttributeUpdates(
            PlacementsSelector selector,
            List<AttributeValuePart> valueParts)
        {
            Selector = selector;
            ValueParts = valueParts;
        }

        public AttributeUpdates(List<AttributeValuePart> valueParts)
            : this(new PlacementsSelector(), valueParts)
        {
        }

        public PlacementsSelector Selector { get; }
        public List<AttributeValuePart> ValueParts { get; }
    }
}