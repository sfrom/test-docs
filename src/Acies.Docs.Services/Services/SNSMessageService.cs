using Acies.Docs.Models.Interfaces;
using Amazon.SimpleNotificationService.Model;
using Common.Models;
using DatabaseContext.Models;
using Notifier;

namespace Acies.Docs.Services.Services
{
    public class SNSMessageService : ISNSMessageService
    {
        private INotificationService notificationService;
        private readonly TenantContext _tenantContext;

        public SNSMessageService(TenantContext tenantContext, INotificationService notificationService)
        {
            this.notificationService = notificationService;
            this._tenantContext = tenantContext;
        }

        public async Task PublishAsync<T>(T data, string resource, EventType evnt, List<KeyValuePair<string, string>> attributes) where T : class
        {
            Console.WriteLine("Publishing status event");
            var info = new NotificationSession
            {
                AccountId = _tenantContext?.AccountId?.ToString(),
                Identity = _tenantContext?.Identity,
                Resource = resource,
                JsonType = JsonType.NewtonSoft
            };

            //KeyValuePair<string, string> statusAttribute = new KeyValuePair<string, string>("Status", status);
            //if (!string.IsNullOrWhiteSpace(outputType))
            //{
            //    KeyValuePair<string, string> outputTypeAttribute = new KeyValuePair<string, string>("OutputType", outputType);
            //    await notificationService.NotifyAsync(data, info, evnt, statusAttribute, outputTypeAttribute);
            //}
            //else
            //{
                await notificationService.NotifyAsync(data, info, evnt, attributes.ToArray());
            //}
        }

        public Task UpdateStatusAsync<T>(T data, string resource, List<KeyValuePair<string, string>> attributes) where T : class
        {
            return PublishAsync(data, resource, EventType.Updated, attributes);
        }
    }
}
