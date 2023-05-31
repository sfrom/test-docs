namespace Acies.Docs.Models
{
    public interface IContentRepository
    {
        Task<string> GetContentAsync(string link);
    }
}
