using Acies.Docs.Models;

namespace Acies.Docs.Services
{
    public class ContentRepository : IContentRepository
    {
        private readonly HttpClient _httpClient;

        public ContentRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetContentAsync(string link)
        {
            return await _httpClient.GetStringAsync(link);
        }
    }
}
