using System;

namespace Sample.Responses
{
    public class LayoutPageResponse
    {
        public Guid LayoutGuid { get; set; }

        public string? Path { get; set; }
        public string? Name { get; set; }
    }
}
