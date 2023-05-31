using Acies.Docs.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Acies.Docs.Services
{
    public class JsonSerializerService : ISerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializerService()
        {
            _options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new TemplateOutputBaseConverter(), new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            };
        }

        public string Serialize(object? data)
        {
            return JsonSerializer.Serialize(data, _options);
        }

        public T? Deserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data, _options);
        }
    }

    public class TemplateOutputBaseConverter : JsonConverter<TemplateOutputBase>
    {
        private readonly JsonSerializerOptions _options;

        public TemplateOutputBaseConverter()
        {
            _options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                PropertyNameCaseInsensitive = true,
            };
        }

        public override bool CanConvert(Type type)
        {
            return typeof(TemplateOutputBase).IsAssignableFrom(type);
        }

        public override TemplateOutputBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                var typeProperty = jsonDocument.RootElement.EnumerateObject()
                      .FirstOrDefault(p => string.Compare(p.Name, "type",
                                                          StringComparison.OrdinalIgnoreCase) == 0);
                if (typeProperty.Value.ValueKind == JsonValueKind.Undefined)
                {
                    throw new JsonException();
                }

                var tpname = typeProperty.Value.GetString();

                var types = new List<Type>
                {
                    typeof(PdfOutput),
                    typeof(StaticOutput),
                    typeof(PngOutput),
                };

                var type = types.FirstOrDefault(x => string.Compare(x.Name, tpname + "Output", true) == 0);
                if (type == null)
                {
                    throw new JsonException();
                }

                var jsonObject = jsonDocument.RootElement.GetRawText();
                var result = (TemplateOutputBase)JsonSerializer.Deserialize(jsonObject, type, _options);
                return result;
            }
        }

        public override void Write(Utf8JsonWriter writer, TemplateOutputBase value, JsonSerializerOptions options)
        {
            if (value is PdfOutput pdfOutput)
            {
                JsonSerializer.Serialize(writer, pdfOutput, _options);
            }
            else if (value is StaticOutput staticOutput)
            {
                JsonSerializer.Serialize(writer, staticOutput, _options);
            }
            else if (value is PngOutput pngOutput)
            {
                JsonSerializer.Serialize(writer, pngOutput, _options);
            }
            else
            {
                throw new NotSupportedException($"Type {value?.Name} not supported for serialization.");
            }
        }
    }
}
