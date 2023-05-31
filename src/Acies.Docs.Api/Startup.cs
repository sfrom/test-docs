using Acies.Docs.Models;
using Acies.Docs.Models.Interfaces;
using Acies.Docs.Services;
using Acies.Docs.Services.Amazon;
using Acies.Docs.Services.Generators;
using Acies.Docs.Services.Repositories;
using Acies.Docs.Services.Services;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Common.Attributes;
using Common.Extensions;
using Common.Middleware;
using Common.Models;
using Logger.Interfaces;
using Logger.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Acies.Docs.Api
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; } = null!;

        private string[] GetAllowedRequestHeaders()
        {
            return new string[]
            {
                "Content-Type",
                "Authorization",
                "Content-Length",
                "Cache-Control",
                "X-Requested-With",
                "x-version",
                AuthorizeAttribute.AccountIdRequiredDefault ? "x-account-id" : ""
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            var envProvider = new DefaultEnvironmentVariableProvider();
            if (envProvider.GetOptionalVariable("AWS_EXECUTION_ENV") != null)
                AWSSDKHandler.RegisterXRayForAllServices();
            else
                AuthorizeAttribute.EnableAuth = false;

            services.AddOptions<DynamoDbDataRepositoryOptions>().Bind(Configuration.GetSection(nameof(DynamoDbDataRepositoryOptions))).ValidateDataAnnotations();
            services.AddScoped(r => r.GetRequiredService<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>().Value);
            services.AddScoped<TenantContext>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().WithHeaders(GetAllowedRequestHeaders()));
            });

            services.AddCoreControllers(config => config.ModelBinderProviders.Insert(0, new CustomModelBinderProvider()));

            services.AddApiVersioning(
                    options =>
                    {
                        // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                        options.ReportApiVersions = true;

                        //Fallback to specific version
                        options.DefaultApiVersion = ApiVersion.Parse("1.0");
                        options.AssumeDefaultVersionWhenUnspecified = true;

                        //Use Header instead
                        options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
                    });

            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });
            services.AddScoped<ILambdaLoggerWrapper, LambdaLoggerWrapper>();
            services.AddScoped<IEnvironmentVariableProvider, DefaultEnvironmentVariableProvider>();
            services.AddScoped<ILogConfig, StaticLogConfig>();
            services.AddScoped<ILogService>(c => new LogService(c.GetService<ILambdaLoggerWrapper>(), c.GetService<IEnvironmentVariableProvider>(), null) { LogConfig = c.GetService<ILogConfig>() });
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<ITemplateRepository, TemplateRepository>();
            services.AddTransient<ISNSMessageService, SNSMessageService>();
            services.AddScoped<IValidationService, ValidationService>();

            services.AddScoped<IAssetService, AssetService>();

            services.AddScoped<IDynamoDBContext>(c => new DynamoDBContext(c.GetRequiredService<IAmazonDynamoDB>()));
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<IOutputService, OutputService>();
            services.AddNotifier(c =>
            {
                c.ServiceName = envProvider.GetVariable("SERVICENAME");
                c.TopicArn = envProvider.GetVariable("SNS");

            });

            //services.AddHttpClient<ContentRepository>(c => c.BaseAddress = new System.Uri("https://cdn.acies.dk"));

            services.AddScoped<ISerializer, JsonSerializerService>();

            //services.AddScoped<IHtmlRenderer, HtmlRenderer>();
            //services.AddScoped<IReadOnlyStreamRepository, S3ReadOnlyStreamRepository>();
            //services.AddScoped<ITransformService, XsltTransformService>();
            //services.AddScoped<IContentRepository, ContentRepository>();
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonS3>();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMiddleware<CoreTenantMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseCoreEndpoints();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
           {
               foreach (var description in provider.ApiVersionDescriptions)
               {
                   options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
               }
           });
        }
    }
}