namespace Acies.Docs.Models
{
    public class StaticOutput : TemplateOutputBase
    {
        public override OutputTypes Type => OutputTypes.Static;
        public Asset? Asset { get; set; }
    }
}