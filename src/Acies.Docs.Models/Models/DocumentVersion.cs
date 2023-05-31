namespace Acies.Docs.Models
{
    public class DocumentVersion
    {
        public string Id { get; set; } = null!;
        public int Version { get; set; }
        public string? Template { get; set; } = null!;
        public int TemplateVersion { get; set; }
        //public Status Status
        //{
        //    get { return Output == null || !Output.Any() ? Status.Succeeded : Output.Min(c => c.Status); }
        //    set { Output.All(c => { c.Status = value; return true; }); }
        //}
        public Status Status { get; set; }
        public string? Input { get; set; }
        public IEnumerable<DocumentOutput>? Output { get; set; } = new List<DocumentOutput>();
        public long CreatedAt { get; set; }
    }
}
