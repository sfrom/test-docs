using Acies.Docs.Models;

namespace Acies.Docs.Services.Generators
{
    //public class StaticOutputGenerator : IOutputGenerator
    //{
    //    private readonly IReadOnlyStreamRepository _templateRepository;
    //    private readonly IWriteOnlyStreamRepository _outputRepository;

    //    public StaticOutputGenerator(IReadOnlyStreamRepository templateRepository, IWriteOnlyStreamRepository outputRepository)
    //    {
    //        _templateRepository = templateRepository;
    //        _outputRepository = outputRepository;
    //    }

    //    public OutputTypes OutputType => OutputTypes.Static;

    //    public async Task GenerateAsync(GeneratorInput generatorInput, string outp, string outputName)
    //    {
    //        var output = generatorInput.TemplateVersion?.Outputs?.FirstOrDefault(c => c.Type == OutputTypes.Pdf && c.Name == outputName) as StaticOutput;
    //        if (output != null && output.Asset != null)
    //        {
    //            await _outputRepository.WriteAsync(await _templateRepository.GetStreamAsync(output.Asset.Path), output.Name);
    //        }
    //    }
    //}
}