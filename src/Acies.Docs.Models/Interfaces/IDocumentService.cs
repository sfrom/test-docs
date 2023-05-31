using DatabaseContext.Models;

namespace Acies.Docs.Models
{
    public interface IDocumentService
    {
        Task<DocumentResponse> CreateDocumentAsync(DocumentCreateData createData);
        Task<IEnumerable<Document>> GetDocumentsByTagsAsync(IDictionary<string, string> tags);
        Task<DocumentVersion?> GetLatestVersionAsync(string key);
        Task<DocumentResponse> UpdateDocumentAsync(string key, DocumentUpdateData updateData);
        Task SetOutputStatus(DocumentVersion documentVersion, string outputName, Status status, string? externalPath = null);
        Task SetDocumentStatus(DocumentVersion documentVersion, Status status);
        Task<DocumentVersion?> GetDocumentVersionByKeyAsync(string key, int version);
        Task<Document?> GetDocumentByKeyAsync(string key);
    }
}