namespace Acies.Docs.Models
{
    public class DocumentCreateData
    {
        public string? TemplateId { get; set; } = null!;
        public int TemplateVersion { get; set; }
        public string? Input { get; set; } = null!;
        public Dictionary<string, string>? Tags { get; set; } = null!;
    }
}
