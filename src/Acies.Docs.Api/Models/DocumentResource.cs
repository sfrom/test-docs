using Acies.Docs.Models;

namespace Acies.Docs.Api.Models
{
    public class DocumentResource : ITaggedItem
    {
        public string Self { get; set; } = string.Empty;
        public SelfObject? Version { get; set; }
        public TemplateRef? Template { get; set; }
        public string? Status { get; set; }
        public string? Input { get; set; }
        public List<SelfObject> Versions { get; set; } = new List<SelfObject>();
        public Dictionary<string, string>? Tags { get; set; }
        public Meta? Meta { get; set; }
    }
}
