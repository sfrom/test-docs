using System.Text;

namespace Acies.Docs.Models
{
    public interface ILinkContentService
    {
        Task<StringBuilder> InsertDrawingContentAsync(string content);
    }
}
