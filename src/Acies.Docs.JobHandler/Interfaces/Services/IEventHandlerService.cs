using static Amazon.Lambda.SNSEvents.SNSEvent;

namespace Acies.Docs.JobHandler.Interfaces.Services
{
    public interface IEventHandlerService
    {
        void ExecuteHandler(string handlerInstanceName, SNSRecord record);
    }
}
