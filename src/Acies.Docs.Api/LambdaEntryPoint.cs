using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Common;
using Logger.Services;

[module: LogInterceptor]
namespace Acies.Docs.Api
{
    public class LambdaEntryPoint : CoreLambdaEntryPoint
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder.UseStartup<Startup>();
        }

        protected override void Init(IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((hostingContext, config) => { config.AddEnvironmentVariables(); });

            builder.ConfigureServices(services =>
            {
                services.AddLogging(b =>
                {
                    b.ClearProviders();
                    b.AddProvider(new CustomLoggerProvider(new LogService(new LambdaLoggerWrapper(), new DefaultEnvironmentVariableProvider(), null) { LogConfig = new StaticLogConfig() }));
                });
            });
        }

        [LogInterceptor]
        public override Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext lambdaContext)
        {
            return base.FunctionHandlerAsync(request, lambdaContext);
        }
    }
}