using static Amazon.Lambda.SNSEvents.SNSEvent;

namespace Acies.Docs.JobHandler.Interfaces.Handlers
{
    public interface IEventHandler
    {
        public string Resource { get; }
        public virtual string Event { get => null; }
        public virtual string Service { get => null; }
        void Execute(SNSRecord record);
    }
}
