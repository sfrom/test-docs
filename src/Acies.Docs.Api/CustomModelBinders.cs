using Acies.Docs.Models;
using Acies.Docs.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Acies.Docs.Api
{

    public class CustomModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(DocumentCreateData))
            {
                return new DocumentCreateDataModelBinder();
            }
            else if (context.Metadata.ModelType == typeof(DocumentUpdateData))
            {
                return new DocumentUpdateDataModelBinder();
            }
            else if (context.Metadata.ModelType == typeof(TemplateCreateData))
            {
                return new TemplateCreateDataModelBinder();
            }
            else if (context.Metadata.ModelType == typeof(DocumentCreateData))
            {
                return new DocumentCreateDataModelBinder();
            }
            return null;
        }
    }

    public class TemplateCreateDataModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new TemplateOutputBaseConverter(), new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            };

            var jsonDocument = await bindingContext.GetBodyAsJsonDocumentAsync();

            if (jsonDocument == null)
                throw new NullReferenceException("Body content");

            var result = new TemplateCreateData();
            foreach (var e in jsonDocument.RootElement.EnumerateObject())
            {
                switch (e.Name.ToLower())
                {
                    case "name":
                        result.Name = e.Value.GetString();
                        break;
                    case "outputs":
                        result.Outputs = e.Value.Deserialize<List<TemplateOutputBase>>(options);
                        break;
                    case "defaultversion":
                        result.Version = e.Value.GetInt64();
                        break;
                    case "tags":
                        result.Tags = e.Value.Deserialize<Dictionary<string, string>>();
                        break;
                    case "input":
                        result.Input = e.Value.Deserialize<TemplateInput>();//JsonConvert.DeserializeObject<TemplateInput>(e.Value.ToString());
                        break;
                }
            }
            bindingContext.Result = ModelBindingResult.Success(result);
        }
    }

    public class TemplateUpdateDataModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var jsonDocument = await bindingContext.GetBodyAsJsonDocumentAsync();

            if (jsonDocument == null)
                throw new NullReferenceException("Body content");

            var result = new TemplateUpdateData();
            foreach (var e in jsonDocument.RootElement.EnumerateObject())
            {
                switch (e.Name.ToLower())
                {
                    case "name":
                        result.Name = e.Value.GetString();
                        break;
                    case "input":
                        result.Input = e.Value.Deserialize<TemplateInput>();
                        //var schema = JSchema.Parse(templInput.Validation);
                        //result.Input = new TemplateInput3() { Validation = schema };
                        break;
                }
            }
            bindingContext.Result = ModelBindingResult.Success(result);
        }
    }

    public class DocumentCreateDataModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var jsonDocument = await bindingContext.GetBodyAsJsonDocumentAsync();

            if (jsonDocument == null)
                throw new NullReferenceException("Body content");

            var result = new DocumentCreateData();
            foreach (var e in jsonDocument.RootElement.EnumerateObject())
            {
                switch (e.Name.ToLower())
                {
                    case "templateversion":
                        result.TemplateVersion = e.Value.GetInt32();
                        break;
                    case "templateid":
                        result.TemplateId = e.Value.GetString();
                        break;
                    case "tags":
                        result.Tags = e.Value.Deserialize<Dictionary<string, string>>();
                        break;
                    case "input":
                        result.Input = e.Value.GetString();
                        break;
                }
            }
            bindingContext.Result = ModelBindingResult.Success(result);
        }
    }

    public class DocumentUpdateDataModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var jsonDocument = await bindingContext.GetBodyAsJsonDocumentAsync();

            if (jsonDocument == null)
                throw new NullReferenceException("Body content");

            var result = new DocumentUpdateData();
            foreach (var e in jsonDocument.RootElement.EnumerateObject())
            {
                switch (e.Name.ToLower())
                {
                    case "templateversion":
                        result.TemplateVersion = e.Value.GetInt32();
                        break;
                    case "templateid":
                        result.TemplateId = e.Value.GetString();
                        break;
                    case "input":
                        result.Input = e.Value.GetString();
                        break;
                }
            }
            bindingContext.Result = ModelBindingResult.Success(result);
        }
    }

    public static class BodyBindingExtensions
    {
        public static async Task<JsonDocument?> GetBodyAsJsonDocumentAsync(this ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            string valueFromBody = string.Empty;

            using (var sr = new StreamReader(bindingContext.HttpContext.Request.Body))
            {
                valueFromBody = await sr.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(valueFromBody))
            {
                return null;
            }
            return JsonDocument.Parse(valueFromBody);
        }
    }
}