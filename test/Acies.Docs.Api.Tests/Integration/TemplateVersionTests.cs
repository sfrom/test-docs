using Acies.Docs.Api.Models;
using Acies.Docs.Models;
using Acies.Docs.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Api.Tests
{
    public class TemplateVersionTests
    {
        private readonly HttpClient _httpClient;

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public TemplateVersionTests()
        {
            Environment.SetEnvironmentVariable("DynamoDbDataRepositoryOptions__TABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
            Environment.SetEnvironmentVariable("SERVICENAME", "docs");
            var webAppFactory = new WebApplicationFactory<LocalEntryPoint>();
            _httpClient = webAppFactory.CreateDefaultClient();
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetTemplateBy_GetTemplateVersion()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var t = new TemplateCreateData
            {
                Name = $"My first template id {id}",
                Version = 1,
                Input = new TemplateInput(),
                Tags = new Dictionary<string, string>
                {
                    { "Type","Order" },
                    { "Id",id },
                },
            };

            var accountId = Guid.NewGuid();

            var templateOutput = CreateTemplate(accountId.ToString());

            //Act
            var response = await _httpClient.GetAsync(templateOutput?.TemplateResource?.Version?.Self);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();
            ISerializer ser = new JsonSerializerService();
            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = ser.Deserialize<TemplateVersionResource>(resultGet.ToString()); //resultGet.ToObject<TemplateVersionResource>();
            Assert.IsType<TemplateVersionResource>(item);
            Assert.StartsWith("/templates/", item?.Self);
            Assert.True(item?.Self.Length == 58);
            Assert.EndsWith("/versions/1", item?.Self);
            Assert.StartsWith("/templates/", item?.Template.Self);
            Assert.True(item?.Template.Self.Length == 47);
            Assert.True(item.Output.Count() == 1);
            Assert.Equal(item.Output.ToArray()[0].Name, "Test", true);
        }

        private TemplateOutput CreateTemplate(string accountId)
        {
            var id = Guid.NewGuid().ToString();
            var t = new TemplateCreateData
            {
                Name = $"My first template id {id}",
                Version = 1,
                Input = new TemplateInput(),
                Tags = new Dictionary<string, string>
                {
                    { "Type","Order" },
                    { "Id",id },
                },
                Outputs = new List<TemplateOutputBase>() { new PdfOutput() { Name = "Test" } }
            };

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId);

            var result = _httpClient.PostAsJsonAsync("/templates", t).GetAwaiter().GetResult();
            var stringResult = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var resultPost = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultPost.ToObject<TemplateResource>();
            var temp = item.Self.Split("/");
            var output = new TemplateOutput() { TemplateId = temp?.Length > 0 ? temp[temp.Length - 1] : "", TemplateResource = item };
            return output;
        }
    }
}