using System;
using System.Collections.Generic;
using Sample.Dtos;

namespace Sample.SampleInfos
{
    internal abstract class SampleInfo
    {
        public abstract string ApiUrl { get; }
        public abstract string Username { get; }
        public abstract string Password { get; }

        public abstract string LayoutFacilityPath { get; }
        public virtual string LayoutPageName => "page01";

        public abstract string SymbolPath { get; }

        public abstract string Identification { get; }
        public abstract List<AttributeValuePart> AttributeValuePartsIdentifying { get; }
        public virtual List<AttributeValuePart> AttributeValuePartsDescriptiveMultilanguage => throw new NotSupportedException();
    }
}