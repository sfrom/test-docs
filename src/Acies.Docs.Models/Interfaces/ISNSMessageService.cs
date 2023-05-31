namespace Acies.Docs.Models.Interfaces
{
    public interface ISNSMessageService
    {
        Task UpdateStatusAsync<T>(T t, string resource, List<KeyValuePair<string, string>> attributes) where T : class;
    }
}
