namespace Acies.Docs.Models
{
    public class DocumentUpdateData
    {
        public string? TemplateId { get; set; } = null!;
        public int TemplateVersion { get; set; }
        public string? Input { get; set; } = null!;
    }
}