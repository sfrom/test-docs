using Acies.Docs.Models;

namespace Acies.Docs.Api.Models
{
    public class TemplateResource: ITaggedItem
    {
        public string Self { get; set; } = String.Empty;
        public string? Name { get; set; }
        public bool Managed { get; set; }
        public SelfObject? Version { get; set; }
        public List<SelfObject> Versions { get; set; } = new List<SelfObject>();
        public Meta Meta { get; set; } = new Meta();
        public Dictionary<string, string>? Tags { get; set; }
    }
}
