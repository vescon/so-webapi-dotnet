using System.Collections.Generic;

namespace Vescon.So.WebApi.Client.Dtos
{
    public class AttributeUpdates
    {
        public AttributeUpdates(
            PlacementsSelector selector,
            List<AttributeValuePart> valueParts,
            Dictionary<string, bool>? overwrittenValues = null)
        {
            Selector = selector;
            ValueParts = valueParts;
            OverwrittenValues = overwrittenValues;
        }

        public AttributeUpdates(List<AttributeValuePart> valueParts)
            : this(new PlacementsSelector(), valueParts)
        {
        }

        public PlacementsSelector Selector { get; }
        public List<AttributeValuePart> ValueParts { get; }

        public Dictionary<string, bool>? OverwrittenValues { get; }
    }
}