namespace Acies.Docs.Api.Models
{
    public class Collection
    {
        public string Self { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public SelfObject? Documents { get; set; }
        public List<Filter>? Filters { get; set; }
        public List<SelfObject>? Groups { get; set; }
        public Meta? Meta { get; set; }
    }
}