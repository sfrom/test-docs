using Acies.Docs.Models;
using Acies.Docs.Models.Interfaces;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Acies.Docs.Services
{
    public class ValidationService : IValidationService
    {
        public ValidationResponse ValidateJson(string? data, string schemaString)
        {
            var schema = new JSchema();
            try
            {
                schema = JSchema.Parse(schemaString);
            }
            catch (Exception)
            {
                return new ValidationResponse() { Success = false, ErrorMessage = "JSON Schema is not valid" };
            }

            try
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    data = "{}";
                }
                var json = JObject.Parse(data);
                var isValid = json.IsValid(schema, out IList<string> errors);
                return new ValidationResponse() { Success = isValid, ErrorMessage = isValid ? "" : "Input data not valid according to schema: " + string.Join(", ", errors) };
            }
            catch(Exception)
            {
                return new ValidationResponse() { Success = false, ErrorMessage = "Input data is not valid JSON"};
            }
        }
    }
}
