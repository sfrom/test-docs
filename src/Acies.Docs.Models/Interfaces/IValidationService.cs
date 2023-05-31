using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acies.Docs.Models.Interfaces
{
    public interface IValidationService
    {
        public ValidationResponse ValidateJson(string? data, string schema);
    }
}
