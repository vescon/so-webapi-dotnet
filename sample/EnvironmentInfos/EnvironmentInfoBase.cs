using System;
using System.Collections.Generic;
using Vescon.So.WebApi.Client.Dtos;

namespace Sample.EnvironmentInfos
{
    internal abstract class EnvironmentInfoBase
    {
        public abstract string ApiUrl { get; }
        public abstract string Username { get; }
        public abstract string Password { get; }

        public abstract string LayoutFacilityPath { get; }
        public virtual string LayoutPageName => "page01";

        public abstract string SymbolPath { get; }
        public abstract string SymbolPathWithConnectors { get; }
        public abstract string MacroPath { get; }

        public abstract string Identification { get; }
        public abstract string IdentificationMainSymbolReference { get; }
        public abstract string IdentificationConnector { get; }
        public abstract string IdentificationConnectorNew { get; }

        public virtual List<AttributeValuePart> AttributeValuePartsIdentifying => throw new NotSupportedException();
        public virtual List<AttributeValuePart> AttributeValuePartsDescriptiveMultilanguage => throw new NotSupportedException();
        public virtual List<AttributeValuePart> AttributeValuePartsPropertyIndexed => throw new NotSupportedException();
    }
}