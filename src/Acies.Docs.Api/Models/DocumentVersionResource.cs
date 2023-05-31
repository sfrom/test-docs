using Acies.Docs.Models;

namespace Acies.Docs.Api.Models
{
    public class DocumentVersionResource
    {
        public string? Self { get; set; }
        public SelfObject? Document { get; set; }
        public TemplateRef? Template { get; set; }
        public string? Status { get; set; }
        public string? Input { get; set; }
        public IEnumerable<OutputResource> Output { get; set; } = new List<OutputResource>();
        public Meta? Meta { get; set; }
    }
}
