using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Acies.Docs.Api.Tests.Integration;

public class AssetsTests
{
    private readonly HttpClient _httpClient;

    public AssetsTests()
    {
        Environment.SetEnvironmentVariable("DynamoDbDataRepositoryOptions__TABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
        Environment.SetEnvironmentVariable("ASSETS_UPLOAD_BUCKET", "sfrdevstackdocs-teststack-168k-assetsuploadbucket-117va7zvrqhz4");
        
        var webAppFactory = new WebApplicationFactory<LocalEntryPoint>();
        _httpClient = webAppFactory.CreateDefaultClient();
        _httpClient
            .DefaultRequestHeaders
            .Add("x-account-id", "ffa75918-1871-43c3-b877-a854bd70bef5");
    }

    [Trait("Category", "DevIntegration")]
    [Fact]
    public async Task GetSignedUri_ReturnsSignedUri()
    {
        // Arrange
        
        var route = $"templates/{Guid.NewGuid()}/assets";
        
        // Act 
        
        var httpResponse = await _httpClient.GetAsync(route);
        var httpContent = httpResponse.Content.ReadAsStringAsync();
        var httpResult = httpContent.Result;
        JObject? result = JsonConvert.DeserializeObject<dynamic>(httpResult)?["result"];
        
        // Assert 
        
        Assert.NotNull(result);
        Assert.NotNull(result!["url"]?.Value<string>());
        Assert.True(result!["expires"]?.Value<int>() > 0);
        Assert.True(Uri.IsWellFormedUriString(result!["url"]?.Value<string>(), UriKind.Absolute));
    }
}