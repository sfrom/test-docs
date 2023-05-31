using Acies.Docs.Api.Models;
using Acies.Docs.Models.Models;

namespace Acies.Docs.Api
{
    public static class Mappers
    {
        public static SignedUrlResource MapSignedUrl(this SignedUrlResponse signed)
        {
            return new SignedUrlResource
            {
                Url = signed.Url,
                MethodType = signed.Method,
                Expires = signed.Expires
            };
        }
        
        public static DocumentResource MapDocument(this Docs.Models.Document document, string urlPrefix)
        {
            var nd = new DocumentResource
            {
                Self = CreateSelf(document.Id, urlPrefix),
                Status = document.Status.ToString().ToLower(),
                Tags = document.Tags,
                Template = new TemplateRef { Version = new SelfObject { Self = $"/templates/{document.Template}/versions/{document.TemplateVersion}" } },
                Version = new SelfObject { Self = CreateVersionSelf(document.Id, document.Version, urlPrefix) },
                Meta = new Meta
                {
                    CreatedAt = document.CreatedAt,
                    Version = document.Version,
                },
            };

            for (var i = 1; i <= document.Version; i++)
            {
                nd.Versions.Add(new SelfObject { Self = CreateVersionSelf(document.Id, i, urlPrefix) });
            }

            return nd;
        }

        public static DocumentVersionResource MapDocumentVersion(this Docs.Models.DocumentVersion document, string urlPrefix)
        {
            var nd = new DocumentVersionResource
            {
                Self = CreateVersionSelf(document.Id, document.Version, urlPrefix),
                Document = new SelfObject { Self = CreateSelf(document.Id, urlPrefix) },
                Template = new TemplateRef { Version = new SelfObject { Self = $"/templates/{document.Template}/versions/{document.TemplateVersion}" } },
                Status = document.Status.ToString().ToLower(),
                Input = document.Input,
                Meta = new Meta
                {
                    CreatedAt = document.CreatedAt,
                    Version = document.Version,
                },
            };

            if (document.Output?.Count() > 0)
            {
                List<OutputResource> outputs = new List<OutputResource>();
                foreach (var outp in document.Output)
                {
                    var item = new OutputResource()
                    {
                        Name = outp.Name,
                        Status = outp.Status.ToString(),
                        Type = outp.Type.ToString(),
                        Self = "/documents/" + document.Id + "/outputs/" + outp.Id
                    };
                    outputs.Add(item);
                }
                nd.Output = outputs;
            }

            return nd;
        }

        public static TemplateResource MapTemplate(this Docs.Models.Template template, string urlPrefix)
        {
            var nd = new TemplateResource
            {
                Self = CreateSelf(template.Id, urlPrefix),
                Version = new SelfObject { Self = CreateVersionSelf(template.Id, template.Version, urlPrefix) },
                Name = template.Name,
                Managed = true,
                Tags = template.Tags,
                Meta = new Meta
                {
                    CreatedAt = template.CreatedAt,
                    Version = template.Version,
                },
            };

            for (var i = 1; i <= template.Version; i++)
            {
                nd.Versions.Add(new SelfObject { Self = CreateVersionSelf(template.Id, i, urlPrefix) });
            }
            return nd;
        }


        public static TemplateVersionResource MapTemplateVersion(this Acies.Docs.Models.TemplateVersion template, string urlPrefix)
        {
            var nd = new TemplateVersionResource
            {
                Self = CreateVersionSelf(template.Id, template.Version, urlPrefix),
                Name = template.Name,
                Template = new SelfObject { Self = CreateSelf(template.Id, urlPrefix) },
                Meta = new Meta
                {
                    CreatedAt = template.CreatedAt,
                    Version = template.Version,
                },
                Input = template.Input,
                Output = template.Outputs,
            };
            return nd;
        }

        private static string CreateVersionSelf(string key, int version, string urlPrefix)
        {
            var components = urlPrefix.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (components.Length == 4)
            {
                if (components[3] == "0")
                {
                    var replaced = urlPrefix.Replace("/versions/0", "/versions/" + version);
                    return replaced;
                }
                else
                {
                    return urlPrefix;
                }
            }
            else if (components.Length == 2)
            {
                return urlPrefix + $"/versions/{version}";
            }
            else if (components.Length == 1)
            {
                return urlPrefix + "/" + key + $"/versions/{version}";
            }
            else
            {
                return "";
            }
        }

        private static string CreateSelf(string key, string urlPrefix)
        {
            var components = urlPrefix.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (components.Length == 1)
            {
                return urlPrefix + "/" + key;
            }
            else if (components.Length == 2)
            {
                return urlPrefix;
            }
            else if (components.Length == 4)
            {
                return "/" + components[0] + "/" + components[1];
            }
            else
            {
                return "";
            }
        }
    }
}