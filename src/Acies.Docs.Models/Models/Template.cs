namespace Acies.Docs.Models
{
    public class Template : ITaggedItem
    {
        public string? Id { get; set; }
        public int Version { get; set; }
        public bool Managed { get; set; }
        public string? Name { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
    }
}