using Common.Models;

namespace Acies.Docs.Api.Models
{
    [Permission]
    public partial class Permission
    {
        [Account] public const string AssetUpload = "AssetUpload";
        [Account] public const string DocumentGet = "DocumentGet";
        [Account] public const string DocumentGetList = "DocumentGetList";
        [Account] public const string DocumentCreate = "DocumentCreate";
        [Account] public const string DocumentUpdate = "DocumentUpdate";
        [Account] public const string RedererGet = "RedererGet";
        [Account] public const string TemplateGet = "TemplateGet";
        [Account] public const string TemplateGetList = "TemplateGetList";
        [Account] public const string TemplateCreate = "TemplateCreate";
        [Account] public const string TemplateUpdate = "TemplateUpdate";
        [Account] public const string TemplateDelete = "TemplateDelete";
    }
}