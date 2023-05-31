using System.IO.Compression;
using System.Net;
using Acies.Docs.JobHandler.Interfaces.Handlers;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Logger.Interfaces;
using Newtonsoft.Json;

namespace Acies.Docs.JobHandler.Handlers;

public class AssetsUploadS3Handler : IAssetsUploadS3Handler
{
    private readonly IAmazonS3 _s3;
    private readonly string _destinationBucket;

    public AssetsUploadS3Handler(IAmazonS3 s3, IEnvironmentVariableProvider provider)
    {
        _destinationBucket = provider.GetVariable("ASSETS_BUCKET");
        _s3 = s3;
    }

    public async Task ExecuteEventHandler(S3Event.S3EventNotificationRecord? record)
    {
        Console.WriteLine($"AssetsUploadS3Handler.ExecuteEventHandler: {JsonConvert.SerializeObject(record?.S3!)}");
        
        var s3Event = record?.S3!;
        var sourceObject = s3Event.Object;
        var sourceBucket = s3Event.Bucket.Name;
        var fileName = Path.GetFileName(sourceObject.Key);
        var prefixPath = Path.GetDirectoryName(sourceObject.Key)!;
        var sourceKey = Path.Combine(prefixPath, fileName);
        
        var response = await _s3.GetObjectAsync(new GetObjectRequest
        {
            Key = sourceKey,
            BucketName = sourceBucket
        });

        var responseStream = response.ResponseStream;
        var fileSize = (int)responseStream.Length;
        var fileStream = new MemoryStream(fileSize);
        await responseStream.CopyToAsync(fileStream);

        Console.WriteLine($"copied assets archive into memory-stream with size of {fileStream.Length} bytes..");

        await UploadArchiveEntriesToS3(fileStream, prefixPath, _destinationBucket);

        fileStream.Position = 0; // Reset position after ZipArchive read
        
        Console.WriteLine($"uploading '{fileName}' with size of {fileStream.Length} bytes");

        await UploadArchiveToS3(fileStream, sourceKey, _destinationBucket);
    }

    private async Task UploadArchiveEntriesToS3(Stream archiveStream, string prefixPath, string destinationBucket)
    {
        using var zip = new ZipArchive(archiveStream, ZipArchiveMode.Read, true);
        foreach (var entry in zip.Entries)
        {
            var entryLength = (int)entry.Length;
            var entryStream = new MemoryStream(entryLength);
            await using (var openEntryStream = entry.Open())
                await openEntryStream.CopyToAsync(entryStream);

            Console.WriteLine($"copied entry '{entry.FullName}' to memory-stream with size of {entryLength} bytes");

            var entryKey = Path.Combine(prefixPath, entry.FullName);

            var upload = await _s3.PutObjectAsync(new PutObjectRequest
            {
                Key = entryKey,
                BucketName = destinationBucket,
                InputStream = entryStream,
                Headers = { ContentLength = entryStream.Length }
            });

            var entryUploadedPrefix = upload.HttpStatusCode.Equals(HttpStatusCode.OK)
                ? "successfully uploaded"
                : "failed to upload";
            
            Console.WriteLine($"{entryUploadedPrefix} '{entryKey}' to {destinationBucket}");
        }
    }

    private async Task UploadArchiveToS3(Stream archiveStream, string archiveKey, string destinationBucket)
    {
        var zipUploaded = await _s3.PutObjectAsync(new PutObjectRequest
        {
            Key = archiveKey,
            BucketName = destinationBucket,
            InputStream = archiveStream,
            Headers = { ContentLength = archiveStream.Length },
            ContentType = "application/zip"
        });

        var fileUploadedPrefix = zipUploaded.HttpStatusCode.Equals(HttpStatusCode.OK)
            ? "successfully uploaded"
            : "failed to upload";
        
        Console.WriteLine($"{fileUploadedPrefix} '{archiveKey}' to {destinationBucket}");
    }
}