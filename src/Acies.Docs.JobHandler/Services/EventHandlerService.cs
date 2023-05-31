using Acies.Docs.JobHandler.Interfaces.Handlers;
using Acies.Docs.JobHandler.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Amazon.Lambda.SNSEvents.SNSEvent;

namespace Acies.Docs.JobHandler.Services
{
    public class EventHandlerService : IEventHandlerService
    {
        private readonly IEnumerable<IEventHandler> eventHandlers;

        public EventHandlerService(IEnumerable<IEventHandler> eventHandlers)
        {
            this.eventHandlers = eventHandlers;
        }
        public void ExecuteHandler(string handlerInstanceName, SNSRecord record)
        {
            var handlers = eventHandlers.Where(h => h.Resource == handlerInstanceName);
            foreach (var handler in handlers)
                handler.Execute(record);
        }
    }
}
