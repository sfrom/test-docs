namespace Acies.Docs.Models
{
    public class DocumentOutput
    {
        public string Id { get; set; } = string.Empty;
        public OutputTypes Type { get; set; } = OutputTypes.None;
        public Status Status { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ExternalPath { get; set; } = string.Empty;
    }
}
