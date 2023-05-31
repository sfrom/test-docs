
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Acies.Docs.Models
{
    public class PdfOutput : TemplateOutputBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public override OutputTypes Type => OutputTypes.Pdf;
        public Layout Layout { get; set; } = null!;
    }

    public enum AssetType
    {
        PlaceHolder = 0,
        Prefix = 1,
        Postfix = 2
    }

    public class Layout
    {
        public string Format { get; set; } = "A4";
        public Margins Margins { get; set; } = new Margins();
        public TemplateRef? Header { get; set; }
        public TemplateRef Body { get; set; } = null!;
        public TemplateRef? Footer { get; set; }
        public List<Asset>? Assets { get; set; }
    }

    public class Margins
    {
        public int Top { get; set; }
        public int Bottom { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
    }

    public class TemplateRef
    {
        public string? Content { get; set; }
        //ToDo skal styles bruges? Og hvordan skal den indsættes og gælde for?
        public string? Style { get; set; }
        public string? ContentFileName { get; set; }
    }

    public class Asset
    {
        public string Path { get; set; } = string.Empty;
        public int Index { get; set; }
        public AssetType Type { get; set; } = AssetType.PlaceHolder;
    }
}