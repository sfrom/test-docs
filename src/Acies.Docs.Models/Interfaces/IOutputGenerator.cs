namespace Acies.Docs.Models
{
    public interface IOutputGenerator
    {
        OutputTypes OutputType { get; }

        Task GenerateAsync(GeneratorInput generatorInput, string output);
    }
}