using Acies.Docs.Models;

namespace Acies.Docs.Services.Generators
{
    //public class PdfOutputGenerator : IOutputGenerator
    //{
    //    private readonly IReadOnlyStreamRepository _templateRepository;
    //    private readonly IWriteOnlyStreamRepository _outputRepository;

    //    public PdfOutputGenerator(IReadOnlyStreamRepository templateRepository,IWriteOnlyStreamRepository outputRepository)
    //    {
    //        _templateRepository = templateRepository;
    //        _outputRepository = outputRepository;
    //    }

    //    public OutputTypes OutputType { get { return OutputTypes.Pdf; } }

    //    public async Task GenerateAsync(GeneratorInput generatorInput, string outp, string outputName)
    //    {
    //        var output=generatorInput.TemplateVersion?.Outputs?.FirstOrDefault(c=>c.Type==OutputTypes.Pdf && c.Name==outputName);
    //        if (output!=null)
    //        {

    //        }


    //    }
    //}
}