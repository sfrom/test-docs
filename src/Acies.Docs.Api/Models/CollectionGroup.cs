namespace Acies.Docs.Api.Models
{
    public class CollectionGroup
    {
        public string Self { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public SelfObject? Collections { get; set; }
        public SelfObject? Documents { get; set; }
        public List<TagRef>? GroupBy { get; set; }
        public List<Filter>? Filters { get; set; }
        public Meta? Meta { get; set; }
    }

    public class TagRef
    {
        public string? Tag { get; set; }
    }
}