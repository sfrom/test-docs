namespace Acies.Docs.Models
{
    public interface ITaggedItem
    {
        Dictionary<string, string>? Tags { get; set; }
    }
}