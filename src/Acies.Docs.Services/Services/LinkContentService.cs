using Acies.Docs.Models;
using System.Text;

namespace Acies.Docs.Services
{
    public class LinkContentService : ILinkContentService
    {
        private readonly IContentRepository _contentRepository;

        public LinkContentService(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }

        public async Task<StringBuilder> InsertDrawingContentAsync(string content)
        {
            var builder = new StringBuilder();

            var endPos = 0;
            var startPos = content.IndexOf("<dwg ", 0, StringComparison.CurrentCultureIgnoreCase);

            while (startPos > -1)
            {
                builder.Append(content[endPos..startPos]);
                endPos = content.IndexOf("</dwg>", startPos, StringComparison.CurrentCultureIgnoreCase);

                if (endPos > -1)
                {
                    endPos += 6;
                    var linkStartPos = content.IndexOf("href=\"", startPos, StringComparison.CurrentCultureIgnoreCase);
                    var linkEndPos = content.IndexOf("\"", linkStartPos + 6, StringComparison.CurrentCultureIgnoreCase);
                    var link = content[(linkStartPos + 6)..linkEndPos];
                    var contentToInsert = await _contentRepository.GetContentAsync(link);
                    if (contentToInsert != null)
                    {
                        contentToInsert = contentToInsert.Replace("class=\"inside\" style=\"display: none\"", "class=\"inside\"");
                        builder.Append(contentToInsert);
                    }
                    else
                    {
                        builder.Append(content[startPos..endPos]);
                    }
                }
                startPos = content.IndexOf("<dwg ", startPos + 1, StringComparison.CurrentCultureIgnoreCase);
            }
            builder.Append(content[endPos..]);
            return builder;
        }
    }
}