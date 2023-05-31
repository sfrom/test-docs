using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acies.Docs.Models
{
    public partial class Document: ITaggedItem
    {
        public string Id { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? Template { get; set; }
        public int TemplateVersion { get; set; }
        public Status Status { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
    }
}
