namespace Acies.Docs.Models
{
    public class TemplateVersion
    {
        public string? Id { get; set; }
        public int Version { get; set; }
        public string? Name { get; set; }
        public TemplateInput? Input { get; set; }
        public List<TemplateOutputBase>? Outputs { get; set; }
        public long CreatedAt { get; set; }
    }
}