namespace Acies.Docs.Models
{
    public interface ITransformService
    {
        string Transform(string data, Stream stream);
    }
}