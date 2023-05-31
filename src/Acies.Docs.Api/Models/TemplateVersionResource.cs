using Acies.Docs.Models;

namespace Acies.Docs.Api.Models
{
    public class TemplateVersionResource
    {
        public string Self { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public SelfObject Template { get; set; } = new();
        public TemplateInput Input { get; set; } = null!;
        public IEnumerable<TemplateOutputBase>? Output { get; set; }
        public Meta Meta { get; set; } = new();

    }
}
