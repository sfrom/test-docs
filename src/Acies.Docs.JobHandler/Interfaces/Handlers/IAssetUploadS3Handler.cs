using Amazon.Lambda.S3Events;

namespace Acies.Docs.JobHandler.Interfaces.Handlers;

public interface IAssetsUploadS3Handler
{
    Task ExecuteEventHandler(S3Event.S3EventNotificationRecord? record);
}
