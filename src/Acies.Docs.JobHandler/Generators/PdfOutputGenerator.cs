using Acies.Docs.Models;
using HandlebarsDotNet;
using Newtonsoft.Json;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using Logger.Interfaces;
using Common.Models;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;
using Acies.Docs.Services.HandlebarHelpers;
using System.Dynamic;
using System.Runtime.CompilerServices;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Text;
using Acies.Docs.JobHandler.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Acies.Docs.JobHandler.Generators;

public class PdfOutputGenerator : IOutputGenerator
{
    private readonly ISerializer _serializer;
    private readonly IDocumentService _documentService;
    private readonly IAmazonS3 _s3;
    private readonly TenantContext _context;
    private readonly string _pdfOutputBucket;
    private readonly string _pdfAssetsBucket;
    private readonly bool _isLocalEnvironment;
    private readonly string _tmpLocation;

    public PdfOutputGenerator(TenantContext context, IEnvironmentVariableProvider variableProvider, ISerializer serializer, IDocumentService documentService, IAmazonS3 s3)
    {
        _context = context;
        _pdfOutputBucket = variableProvider.GetVariable("RESOURCE_BUCKET");
        _pdfAssetsBucket = variableProvider.GetVariable("ASSETS_BUCKET");
        _isLocalEnvironment = variableProvider.GetOptionalVariable("AWS_EXECUTION_ENV") is null;
        _serializer = serializer;
        _documentService = documentService;
        _s3 = s3;
        _tmpLocation = "/tmp";

        HandlebarInjectionHelper.RegisterHelpers();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public OutputTypes OutputType => OutputTypes.Pdf;

    [LogExecutionTime]
    public async Task GenerateAsync(GeneratorInput generatorInput, string output)
    {
        var pdfOutput = _serializer.Deserialize<PdfOutput>(output);
        if (pdfOutput is null) return;

        var documentVersion = generatorInput.DocumentVersion;

        var documentOutput = documentVersion.Output?.FirstOrDefault(e => e.Type.Equals(OutputType));
        if (documentOutput is null) return;

        var data = documentVersion.Input;

        await _documentService.SetOutputStatus(documentVersion, pdfOutput.Name, Status.Processing);

        if (_isLocalEnvironment)
        {
            await GetAssetsFromFolderToProjectDirectory("assets");
        }
        else
        {
            await GetAssetsFromS3ToTempDirectory(pdfOutput.Layout.Assets, _pdfAssetsBucket,$@"accounts/{_context.AccountId}/templates/{generatorInput.TemplateVersion?.Id}/assets");
        }

        MemoryStream pdfStream = new();

        try
        {
            var stream = await GeneratePdfOutputAsync(data, pdfOutput.Layout);
            var generated = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            var document = await MergeExistingPdfDocumentsIntoGeneratedDocument(
                pdfOutput.Layout.Assets,
                generated
            );

            document.Save(pdfStream, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{GetMethodName()}] exception message: {ex.Message}", ex);
            await _documentService.SetOutputStatus(documentVersion, pdfOutput.Name, Status.Failed);
            return;
        }
        finally
        {
            ClearAllFilesFromLambdaTmpDirectory(_tmpLocation);
        }

        var bucket = $@"{_pdfOutputBucket}/accounts/{_context.AccountId}/documents/{documentVersion.Id}/outputs/{documentOutput.Id}";
        var putPdfRequest = await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucket,
            Key = AppendWhenMissing($"{pdfOutput.Name}".ToLower(), ".pdf"),
            InputStream = pdfStream,
            ContentType = "application/pdf"
        });

        if (putPdfRequest.HttpStatusCode == HttpStatusCode.OK)
        {
            Console.WriteLine($"[{GetMethodName()}] pdf {pdfOutput.Name} uploaded to bucket {_pdfOutputBucket}");
            await _documentService.SetOutputStatus(documentVersion, pdfOutput.Name, Status.Succeeded, bucket);
            return;
        }

        Console.WriteLine($"[{GetMethodName()}] could not upload pdf {pdfOutput.Name} to bucket {_pdfOutputBucket}");
        await _documentService.SetOutputStatus(documentVersion, pdfOutput.Name, Status.Failed);
    }
    
    [LogExecutionTime]
    private void ClearAllFilesFromLambdaTmpDirectory(string path)
    {
        if (_isLocalEnvironment || !Directory.Exists(path)) return;
        var files = Directory.GetFiles(path, "", SearchOption.AllDirectories).ToList();
        Console.WriteLine($"[{GetMethodName()}] cleaning {files.Count} files from tmp...");
        files.ForEach(File.Delete);
    }

    #region Only used when running unit tests and integration tests locally

    private static async Task GetAssetsFromFolderToProjectDirectory(string assetPath)
    {
        var currentDir = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(currentDir)?.Parent?.Parent?.FullName!;
        var existingAssetsPath = Path.Combine(projectDirectory, assetPath);
        var existingAssets = Directory.GetFiles(Path.Combine(projectDirectory, assetPath), "", SearchOption.AllDirectories);
        foreach (var file in existingAssets)
        {
            var fileContent = await File.ReadAllBytesAsync(file);
            var fileSubPath = file.Substring(existingAssetsPath.Length + 1, file.Length - existingAssetsPath.Length - 1);

            var newAssetsPath = Path.Combine(currentDir, fileSubPath);
            var newAssetsDirectory = Path.GetDirectoryName(newAssetsPath)!;

            Directory.CreateDirectory(newAssetsDirectory);

            await File.WriteAllBytesAsync(newAssetsPath, fileContent);
        }
    }

    #endregion

    [LogExecutionTime]
    private async Task GetAssetsFromS3ToTempDirectory(List<Asset>? assets, string bucket, string suffixPath)
    {
        if (assets is null || !assets.Any()) return;

        foreach (var asset in assets)
        {
            var key = Path.Combine(suffixPath, asset.Path);

            MemoryStream readStream = new();
            using (var getObjectResponse = await _s3.GetObjectAsync(new GetObjectRequest { BucketName = bucket, Key = key })) 
                await getObjectResponse.ResponseStream.CopyToAsync(readStream);

            var destinationDirectory = $"{Path.GetDirectoryName(asset.Path)}";
            var tmpPath = Path.Combine(_tmpLocation, asset.Path);
            var tmpDirectory = Path.Combine(_tmpLocation, destinationDirectory);

            if (!string.IsNullOrWhiteSpace(destinationDirectory) && !Directory.Exists(tmpDirectory))
            {
                Directory.CreateDirectory(tmpDirectory);
            }

            if (readStream.Length == 0) continue;

            await File.WriteAllBytesAsync(tmpPath, readStream.ToArray());

            Console.WriteLine($"[{GetMethodName()}] wrote '{asset.Path}' stream of {readStream.Length} bytes to '{tmpPath}'");
        }
    }

    [LogExecutionTime]
    private Task<PdfDocument> MergeExistingPdfDocumentsIntoGeneratedDocument(List<Asset>? assets, PdfDocument pdf)
    {
        Console.WriteLine($"[{GetMethodName()}] merging {assets?.Count(e => !e.Type.Equals(AssetType.PlaceHolder))} assets..");

        if (assets is null || assets.All(e => e.Type.Equals(AssetType.PlaceHolder)))
        {
            return Task.FromResult(pdf);
        }

        var generating = new PdfDocument();

        foreach (var asset in assets.Where(e => e.Type.Equals(AssetType.Prefix)).OrderBy(e => e.Index))
        {
            Console.WriteLine($"[{GetMethodName()}] merging pre-fixed asset {asset.Path}..");
            MergeExistingPdfDocumentIntoGeneratedDocument(asset, generating);
        }

        Console.WriteLine($"[{GetMethodName()}] merging generated document..");
        foreach (var page in pdf.Pages)
        {
            generating.Pages.Add(page);
        }

        foreach (var asset in assets.Where(e => e.Type.Equals(AssetType.Postfix)).OrderBy(e => e.Index))
        {
            Console.WriteLine($"[{GetMethodName()}] merging post-fixed asset {asset.Path}..");
            MergeExistingPdfDocumentIntoGeneratedDocument(asset, generating);
        }

        return Task.FromResult(generating);
    }

    [LogExecutionTime]
    private void MergeExistingPdfDocumentIntoGeneratedDocument(Asset asset, PdfDocument document)
    {
        var path = Path.Combine(
            _isLocalEnvironment 
                ? Environment.CurrentDirectory + _tmpLocation
                : _tmpLocation, 
            asset.Path
        );
        
        var pages = PdfReader.Open(path, PdfDocumentOpenMode.Import).Pages;

        foreach (var page in pages)
        {
            document.Pages.Add(page);
        }
    }

    [LogExecutionTime]
    public async Task<Stream> GeneratePdfOutputAsync(string? data, Layout layout)
    {
        dynamic? input = JsonConvert.DeserializeObject<ExpandoObject>($"{data}");
        var headerHtml = MergeInputDataAndCompileHtmlTemplate(layout.Header, input);
        var bodyHtml = MergeInputDataAndCompileHtmlTemplate(layout.Body, input);
        var footerHtml = MergeInputDataAndCompileHtmlTemplate(layout.Footer, input);
        return await GeneratePdfStreamInternalAsync(layout, headerHtml, bodyHtml, footerHtml);
    }

    [LogExecutionTime]
    public async Task<Stream> GeneratePdfStreamInternalAsync(Layout layout, string header, string body, string footer)
    {
        var options = new PdfOptions
        {
            DisplayHeaderFooter = true,
            FooterTemplate = footer,
            HeaderTemplate = header,
            Format = ConvertPdfFormatFromString(layout.Format),
            MarginOptions = new MarginOptions
            {
                Bottom = $"{layout.Margins.Bottom}",
                Left = $"{layout.Margins.Left}",
                Right = $"{layout.Margins.Right}",
                Top = $"{layout.Margins.Top}"
            },
            OmitBackground = false,
            PrintBackground = true
        };

        Console.WriteLine($"[{GetMethodName()}] generating {(_isLocalEnvironment ? "windows" : "linux")} stream..");

        return _isLocalEnvironment
            ? await GetWindowsPdfStream(options, body)
            : await GetLinuxPdfStream(options, body);
    }

    #region Only used when running unit tests and integration tests locally

    private static async Task<Stream> GetWindowsPdfStream(PdfOptions options, string body)
    {
        BrowserFetcher browserFetcher = new();
        await browserFetcher.DownloadAsync();
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(body, new NavigationOptions());
        return await page.PdfStreamAsync(options);
    }

    #endregion

    private static async Task<Stream> GetLinuxPdfStream(PdfOptions options, string body)
    {
        const string httpLocalhost = "http://localhost:50001";
        await File.WriteAllTextAsync("/tmp/generated.html", body);

        Stream stream = new MemoryStream();

        try
        {
            using var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls(httpLocalhost)
                .UseStartup<StartupDirectoryApi>()
                .Build();
            
            using var runner = host.RunAsync();
            var browserLauncher = new HeadlessChromiumPuppeteerLauncher(new LoggerFactory());
            await using var browser = await browserLauncher.LaunchAsync();
            await using var page = await browser.NewPageAsync();
            await page.SetCacheEnabledAsync(false); // Speed optimization
            await page.GoToAsync($"{httpLocalhost}/tmp/generated.html", GetNavigationOptions());
            stream = await page.PdfStreamAsync(options);
            host.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[{GetMethodName()}] exception: {e.Message}", e);
        }

        Console.WriteLine($"[{GetMethodName()}] got stream from {httpLocalhost}/tmp/generated.html with size of {stream.Length} bytes");
        
        return stream;
    }

    private static PaperFormat ConvertPdfFormatFromString(string format)
    {
        return format switch
        {
            "Letter" => PaperFormat.Letter,
            "Legal" => PaperFormat.Legal,
            "Tabloid" => PaperFormat.Tabloid,
            "Ledger" => PaperFormat.Ledger,
            "A0" => PaperFormat.A0,
            "A1" => PaperFormat.A1,
            "A2" => PaperFormat.A2,
            "A3" => PaperFormat.A3,
            "A4" => PaperFormat.A4,
            "A5" => PaperFormat.A5,
            "A6" => PaperFormat.A6,
            _ => PaperFormat.A4
        };
    }

    public static string MergeInputDataAndCompileHtmlTemplate(TemplateRef? template, dynamic data)
    {
        return template?.Content is null
            ? string.Empty
            : Handlebars.Compile($"{StylesheetTagsWhenMissing(template.Style)}{template.Content}")(data);
    }

    public static string StylesheetTagsWhenMissing(string? style)
    {
        style = $"{style}".Trim();

        if (style == "" || new string(style.Take(6).ToArray()).ToUpper().StartsWith("<STYLE"))
        {
            return style;
        }

        return $"<style type='text/css'> {style} </style>";
    }

    private static string AppendWhenMissing(string input, string prefix)
    {
        return input.EndsWith(prefix) ? input : input + prefix;
    }

    private static NavigationOptions GetNavigationOptions()
    {
        return new NavigationOptions
        {
            WaitUntil = new[]
            {
                WaitUntilNavigation.DOMContentLoaded
            }
        };
    }

    private static string GetMethodName([CallerMemberName] string name = "")
    {
        return name;
    }
}

public class StartupDirectoryApi
{
    public static void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(@"/tmp"),
            RequestPath = new PathString("/tmp")
        });

        app.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = new PhysicalFileProvider("/tmp"),
            RequestPath = new PathString("/tmp")
        });
    }
}