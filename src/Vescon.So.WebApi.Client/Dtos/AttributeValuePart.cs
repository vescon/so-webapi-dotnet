﻿namespace Vescon.So.WebApi.Client.Dtos
{
    public class AttributeValuePart
    {
        /// <summary>
        /// required for JSON deserialization from .NET 4.7.2
        /// </summary>
        public AttributeValuePart()
        {
        }

        public AttributeValuePart(string name, string value)
            : this(name)
        {
            Value = value;
        }

        public AttributeValuePart(string name, string language, string value, bool isDescription = false)
            : this(name)
        {
            Language = language;

            if (isDescription)
                Description = value;
            else
                Value = value;
        }

        public AttributeValuePart(string name, int index, string value)
            : this(name)
        {
            Index = index;
            Value = value;
        }
        
        private AttributeValuePart(string name)
        : this()
        {
            Name = name;
        }

        /// <summary>
        /// Key part name.
        /// </summary>
        public string Name { get; set; }

        public string? Language { get; set; }
        public int? Index { get; set; }

        public string? Value { get; set; }

        /// <summary>
        /// Optional and only for identifying values.
        /// </summary>
        public string? Description { get; set; }
    }
}