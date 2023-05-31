namespace Acies.Docs.Models
{
    public class TemplateCreateData
    {
        public string? Name { get; set; } = null!;
        public TemplateInput? Input { get; set; }
        public List<TemplateOutputBase>? Outputs { get; set; } = null!;
        public Dictionary<string, string>? Tags { get; set; }
        public long? Version { get; set; } = null!;
    }
}
