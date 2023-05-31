using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acies.Docs.Models
{
    public class SignedUrl
    {
        public string Url { get; set; }
        public string Key { get; set; }
        public long Expires { get; set; }
        public string MethodType { get; set; }
        public Dictionary<string, string> RequiredHeaders { get; set; }
    }
}
