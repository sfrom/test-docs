namespace Acies.Docs.Models
{
    public abstract class TemplateOutputBase : ITaggedItem
    {
        public abstract OutputTypes Type { get; }
        public string Name { get; set; } = null!;
        public Dictionary<string, string>? Tags { get; set; }
    }
}