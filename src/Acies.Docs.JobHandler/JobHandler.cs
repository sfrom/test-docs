using Acies.Docs.JobHandler.Extensions;
using Acies.Docs.JobHandler.Handlers;
using Acies.Docs.JobHandler.Interfaces.Handlers;
using Acies.Docs.JobHandler.Interfaces.Services;
using Acies.Docs.JobHandler.Services;
using Acies.Docs.Models;
using Acies.Docs.Models.Interfaces;
using Acies.Docs.Services;
using Acies.Docs.Services.Amazon;
using Acies.Docs.Services.Repositories;
using Acies.Docs.Services.Services;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Common.Models;
using Logger.Interfaces;
using Logger.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
[module: LogInterceptor]
namespace Acies.Docs.JobHandler;

public class JobHandler
{
    private IServiceProvider serviceProvider;
    //private ILambdaConfiguration configService;

    //private ISecretsManagerService _secretsManagerService;
    private ILogService logger;
    private ILogConfig logConfig;

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public JobHandler()
    {
        // Set up Dependency Injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        serviceProvider = serviceCollection.BuildServiceProvider();

        // Get Configuration Service from DI system
        //configService = serviceProvider.GetService<ILambdaConfiguration>();

        logger = serviceProvider.GetService<ILogService>();
        logConfig = serviceProvider.GetService<ILogConfig>();
        LogInterceptor.UseNewtonsoft = true;
    }

    private void ConfigureServices(IServiceCollection serviceCollection)
    {
        var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile($"appsettings.development.json", true)
                .AddEnvironmentVariables()
                .Build();

        var envProvider = new DefaultEnvironmentVariableProvider();

        var awsEnv = envProvider.GetOptionalVariable("AWS_EXECUTION_ENV");
        var isLocal = awsEnv.StartsWith("AWS_DOTNET_LAMDBA_TEST_TOOL") || awsEnv == null;

        if (!isLocal)
            AWSSDKHandler.RegisterXRayForAllServices();

        serviceCollection.AddTransient<ILambdaLoggerWrapper, LambdaLoggerWrapper>();
        serviceCollection.AddTransient<IEventHandlerService, EventHandlerService>();
        serviceCollection.AddTransient<IAssetsUploadS3Handler, AssetsUploadS3Handler>();
        serviceCollection.RegisterAllTypes<IEventHandler>(new[] { typeof(JobHandler).Assembly });
        serviceCollection.RegisterAllTypes<IOutputGenerator>(new[] { typeof(JobHandler).Assembly });

        serviceCollection.AddTransient<ILogConfig, StaticLogConfig>();
        serviceCollection.AddScoped<ISerializer, JsonSerializerService>();
        serviceCollection.AddScoped<IAmazonDynamoDB, AmazonDynamoDBClient>();
        serviceCollection.AddScoped<IDynamoDBContext, DynamoDBContext>();
        serviceCollection.AddTransient<IEnvironmentVariableProvider>(s => envProvider);
        serviceCollection.AddScoped<IDocumentService, DocumentService>();

        serviceCollection.AddOptions<DynamoDbDataRepositoryOptions>().Bind(configuration.GetSection(nameof(DynamoDbDataRepositoryOptions))).ValidateDataAnnotations();
        serviceCollection.AddScoped(r => r.GetRequiredService<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>().Value);
        serviceCollection.AddScoped<TenantContext>();

        serviceCollection.AddScoped<IDocumentRepository, DocumentRepository>();
        serviceCollection.AddScoped<ITemplateService, TemplateService>();
        serviceCollection.AddScoped<ITemplateRepository, TemplateRepository>();
        serviceCollection.AddScoped<ISNSMessageService, SNSMessageService>();
        serviceCollection.AddTransient<ILogService>(m => new LogService(m.GetService<ILambdaLoggerWrapper>(), m.GetService<IEnvironmentVariableProvider>(), null) { LogConfig = m.GetService<ILogConfig>() });
        serviceCollection.AddNotifier(c =>
        {
            //c.BucketName = envProvider.GetVariable("EVENTBUCKET");
            c.ServiceName = envProvider.GetVariable("SERVICENAME");
            c.TopicArn = envProvider.GetVariable("SNS");
        });

        serviceCollection.AddLogging(builder =>
        {
            builder.AddFilter("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning);
            builder.AddFilter("System", Microsoft.Extensions.Logging.LogLevel.Error);
        });
    }


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SNS event object and can be used 
    /// to respond to SNS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public void FunctionHandler(SNSEvent evnt, ILambdaContext context)
    {
        foreach(var record in evnt.Records)
        {
            ProcessRecordAsync(record, context);
        }
    }
    
    public void FunctionHandlerS3(SQSEvent sqsEvent, ILambdaContext context)
    {
        foreach (var record in sqsEvent.Records)
        {
            ProcessS3RecordAsync(record, context);
        }
    }

    private void ProcessS3RecordAsync(SQSEvent.SQSMessage record, ILambdaContext context)
    {
        var s3Event = JsonConvert.DeserializeObject<S3Event>(record.Body);
        var s3EventRecord = s3Event?.Records.First();

        context.Logger.LogInformation($"JobHandler ProcessS3RecordAsync...");
        context.Logger.LogInformation($"Event: {s3EventRecord!.EventName}");
        context.Logger.LogInformation($"Key: {s3EventRecord!.S3.Object.Key}");

        serviceProvider.GetService<IAssetsUploadS3Handler>()!.ExecuteEventHandler(s3EventRecord).GetAwaiter().GetResult();
    }

    private void ProcessRecordAsync(SNSEvent.SNSRecord record, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processed record {record.Sns.Message}");

        using (var scope = serviceProvider.CreateScope())
        {
            var jobHandlerService = scope.ServiceProvider.GetService<IEventHandlerService>();

            var resourceName = record.Sns.MessageAttributes["Resource"];
            if (resourceName != null && !string.IsNullOrWhiteSpace(resourceName.Value))
                jobHandlerService.ExecuteHandler(resourceName.Value, record);
            else
                throw new Exception("Resource not specified");
        }
    }
}