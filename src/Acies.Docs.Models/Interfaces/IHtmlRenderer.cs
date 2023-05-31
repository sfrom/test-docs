namespace Acies.Docs.Models
{
    public interface IHtmlRenderer
    {
        Task<string> GenerateAsync<T>(GeneratorInput generatorInput) where T : TemplateOutputBase;
    }
}