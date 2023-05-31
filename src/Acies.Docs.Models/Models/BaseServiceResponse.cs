using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acies.Docs.Models
{
    public class BaseServiceResponse
    {
        public string ErrorMessage { get; set; }
        public bool Success { get; set; } = true;
    }
}
