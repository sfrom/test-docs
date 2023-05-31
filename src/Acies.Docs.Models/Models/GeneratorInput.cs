namespace Acies.Docs.Models
{
    public class GeneratorInput
    {
        public DocumentVersion DocumentVersion { get; set; } = null!;
        public Template Template { get; set; } = null!;
        public TemplateVersion? TemplateVersion { get; set; } = null!;
        //public string OutputId { get; set; } = "";
    }
}