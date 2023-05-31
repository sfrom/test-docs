namespace Acies.Docs.Api.Models
{
    public class TemplateRef
    {
        public SelfObject? Version { get; set; }
    }

    public class Meta
    {
        public int Version { get; set; }
        public long CreatedAt { get; set; }
    }

    public class SelfObject
    {
        public string? Self { get; set; }
    }

    public class Filter
    {
        public string Tag { get; set; } = string.Empty;
        public string? Operator { get; set; }
        public string? Value { get; set; }
    }
}
