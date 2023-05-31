using Acies.Docs.JobHandler.Interfaces.Handlers;
using Amazon.Lambda.SNSEvents;
using DatabaseContext.Models;
using Notifier;

namespace Acies.Docs.JobHandler.Handlers
{
    public abstract class BaseEventHandler<T> : IEventHandler
    {
        private INotificationService notificationService;
        private string accountId;
        private Guid userId;
        private string correlationId;
        private string debug;
        protected AuthObj Auth { get; set; }
        public BaseEventHandler(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        public abstract string Resource { get; }
        public virtual string Service { get => null; }

        public void Execute(SNSEvent.SNSRecord record)
        {
            var item = notificationService.UnpackRecord<T>(record, JsonType.NewtonSoft);
            accountId = item.NotificatinSession.AccountId;
            userId = item.NotificatinSession.Identity != null ? item.NotificatinSession.Identity.Id : new Guid();
            correlationId = item.NotificatinSession.CorrelationId;
            debug = item.NotificatinSession.Debug;

            Auth = new AuthObj();
            if (Guid.TryParse(accountId, out Guid accountIdGuid))
                Auth.AccountId = accountIdGuid;
            if (Guid.TryParse(userId.ToString(), out Guid userIdGuid))
                Auth.Identity = userIdGuid;

            var eventName = record.Sns.MessageAttributes["Event"].Value;
            if (eventName == "Created")
                OnCreate(item.Data, GetAttributes(record));
            else if (eventName == "Updated")
                OnUpdate(item.Data, GetAttributes(record));
            else if (eventName == "Deleted")
                OnDelete(item.Data, GetAttributes(record));
            else if (eventName == "Refresh")
                OnRefresh(item.Data, GetAttributes(record));
        }

        protected virtual void OnCreate(T item, IDictionary<string, string> attributes) { }
        protected virtual void OnUpdate(T item, IDictionary<string, string> attributes) { }
        protected virtual void OnDelete(T item, IDictionary<string, string> attributes) { }
        protected virtual void OnRefresh(T item, IDictionary<string, string> attributes) { }

        protected Task PublishEventAsync<E>(E t, string resourceName, Guid? accountId = null, EventType? type = null) where E : class
        {
            var session = new NotificationSession
            {
                AccountId = accountId?.ToString() ?? this.accountId,
                Resource = resourceName,
                Identity = Common.Auth.Identity.FromUser(userId),
                CorrelationId = correlationId,
                Debug = debug,
                JsonType = JsonType.NewtonSoft
            };

            return notificationService.NotifyAsync(t, session, type ?? EventType.Created);
        }

        protected void PublishEvent<E>(E t, string resourceName) where E : class
        {
            PublishEventAsync(t, resourceName, null).Wait();
        }

        protected void PublishEvent<E>(E t, string resourceName, Guid? accountId = null) where E : class
        {
            PublishEventAsync(t, resourceName, accountId).Wait();
        }

        protected void PublishEvent<E>(E t, string resourceName, EventType? type = null) where E : class
        {
            PublishEventAsync(t, resourceName, type: type).Wait();
        }

        private IDictionary<string, string> GetAttributes(SNSEvent.SNSRecord record)
        {
            return record.Sns.MessageAttributes.ToDictionary(s => s.Key, s => s.Value.Value);
        }
    }
}
