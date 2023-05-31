namespace Acies.Docs.Models
{
    public interface ISerializer
    {
        T? Deserialize<T>(string data);
        string Serialize(object? data);
    }
}