using System.Collections.Generic;

namespace Acies.Docs.Api.Models
{
    public class OutputResource
    {
        public string? Self { get; set; }
        public string? Status { get; set; }
        public string Type { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
