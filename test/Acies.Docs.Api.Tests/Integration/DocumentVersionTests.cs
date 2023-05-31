using Acies.Docs.Api.Models;
using Acies.Docs.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Api.Tests
{
    public class DocumentVersionTests
    {
        private readonly HttpClient _httpClient;

        public DocumentVersionTests()
        {
            Environment.SetEnvironmentVariable("DynamoDbDataRepositoryOptions__TABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
            Environment.SetEnvironmentVariable("SNS", "arn:aws:sns:eu-central-1:864358974821:sfrdevstackdocs-LocalSNS-15C72KmVXr6B");
            Environment.SetEnvironmentVariable("SERVICENAME", "docs");
            var webAppFactory = new WebApplicationFactory<LocalEntryPoint>();
            _httpClient = webAppFactory.CreateDefaultClient();
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetDocumentBy_GetDocument()
        {
            //Arrange
            var ordernumber = Guid.NewGuid().ToString();

            dynamic data = new ExpandoObject();
            data.Name = "Jens Jensen";
            data.OrderNumber = ordernumber;
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var accountId = Guid.Parse("06b12620-d104-4946-8e28-b53219ded520");

            var templateOutput = CreateTemplate(accountId.ToString());

            var d = new DocumentCreateData
            {
                TemplateId = templateOutput.TemplateId,
                TemplateVersion = 1,
                Tags = new Dictionary<string, string>
                {
                    { "Type","Order" },
                    { "Number",ordernumber },
                },
                Input = System.Text.Json.JsonSerializer.Serialize(data),
            };

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            
            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            var responsePost = await _httpClient.PostAsJsonAsync("/documents", d);
            var stringResultPost = await responsePost.Content.ReadAsStringAsync();
            var resultPost = JsonConvert.DeserializeObject<JObject>(stringResultPost, settings)["result"];
            var itemPost = resultPost.ToObject<DocumentResource>();

            //Act
            var response = await _httpClient.GetAsync(itemPost?.Version?.Self);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();

            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultGet.ToObject<DocumentVersionResource>();
            Assert.IsType<DocumentVersionResource>(item);
            //Assert.EndsWith(item?.Self, responsePost.Headers.Location?.ToString() + "/versions/1");
            Assert.StartsWith("/documents/", item?.Self);
            Assert.True(item?.Self.Length == 58);
            Assert.EndsWith("/versions/1", item?.Self);
            Assert.StartsWith("/documents/", item?.Document.Self);
            Assert.False(item?.Document.Self.Contains("versions"));
            Assert.True(item?.Document.Self.Length == 47);
            Assert.True(item.Output.Count() == 1);
            Assert.Equal(item.Output.ToArray()[0].Name, "Test.pdf", true);
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
                Outputs = new List<TemplateOutputBase>() { new PdfOutput() { Name = "Test"} }
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