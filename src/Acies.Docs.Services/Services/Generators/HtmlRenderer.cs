using Acies.Docs.Models;
using Acies.Docs.Models.Exceptions;
using System.Text;

namespace Acies.Docs.Services.Generators
{
    public class HtmlRenderer : IHtmlRenderer
    {
        private readonly IReadOnlyStreamRepository _templateRepository;
        private readonly ITransformService _transformService;
        private readonly IContentRepository _contentRepository;

        public HtmlRenderer(IReadOnlyStreamRepository templateRepository, ITransformService transformService, IContentRepository contentRepository)
        {
            _templateRepository = templateRepository;
            _transformService = transformService;
            _contentRepository = contentRepository;
        }

        public async Task<string> GenerateAsync<T>(GeneratorInput generatorInput) where T : TemplateOutputBase
        {
            var outputType = generatorInput.TemplateVersion?.Outputs?.SingleOrDefault(c => c is T);
            if (outputType != null && outputType is PdfOutput pdfOutput)
            {
                if (pdfOutput != null)
                {
                    var content = await GetContentAsync(pdfOutput.Layout.Body);
                    if (content == null) throw new NullReferenceException("No content body or filename set for Pdf output.");
                    if (content.ContentType == ContentType.XSLT)
                    {
                        var html = _transformService.Transform(generatorInput.DocumentVersion.Input, content.Stream);
                        return await _contentRepository.GetContentAsync(html);
                    }
                    else
                    {
                        //Kald Node Vue.
                        return "";
                    }
                }
            }
            throw new UnsupportedOutputTypeException(typeof(T).Name);
        }

        private async Task<Content?> GetContentAsync(TemplateRef templateRef)
        {
            if (!string.IsNullOrEmpty(templateRef.ContentFileName))
            {
                var content = new Content
                {
                    Stream = await _templateRepository.GetStreamAsync(templateRef.ContentFileName),
                };
                if (string.Compare(System.IO.Path.GetExtension(templateRef.ContentFileName), "html", true) == 0) content.ContentType = ContentType.Html;
                else if (string.Compare(System.IO.Path.GetExtension(templateRef.ContentFileName), "xslt", true) == 0) content.ContentType = ContentType.XSLT;
                else if (string.Compare(System.IO.Path.GetExtension(templateRef.ContentFileName), "xsl", true) == 0) content.ContentType = ContentType.XSLT;
                return content;
            }
            else if (!string.IsNullOrEmpty(templateRef.Content))
            {
                var content = new Content
                {
                    Stream = ToStream(templateRef.Content),
                };
                if (templateRef.Content.IndexOf("<xsl:stylesheet")>-1) content.ContentType = ContentType.XSLT;
                else content.ContentType = ContentType.Html;
                return content;
            }
            else
            {
                return null;
            }
        }

        private Stream ToStream(string text)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(text);
            return new MemoryStream(byteArray);
        }

        internal class Content
        {
            public ContentType ContentType { get; set; }
            public Stream? Stream { get; set; }
        }
    }
}