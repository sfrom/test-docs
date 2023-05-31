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
    public class DocumentTests
    {
        private readonly HttpClient _httpClient;

        public DocumentTests()
        {
            Environment.SetEnvironmentVariable("DynamoDbDataRepositoryOptions__TABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
            Environment.SetEnvironmentVariable("SNS", "arn:aws:sns:eu-central-1:864358974821:sfrdevstackdocs-LocalSNS-15C72KmVXr6B");
            Environment.SetEnvironmentVariable("SERVICENAME", "docs");
            var webAppFactory = new WebApplicationFactory<LocalEntryPoint>();
            _httpClient = webAppFactory.CreateDefaultClient();
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetDocumentsByTags_GetOne()
        {
            //Arrange
            var ordernumber = Guid.NewGuid().ToString();

            dynamic data = new ExpandoObject();
            data.Name = "Jens Jensen";
            data.OrderNumber = ordernumber;
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var accountId = Guid.NewGuid();

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
                Input = ""
            };

            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());
            var responsePost = await _httpClient.PostAsJsonAsync("/documents", d);

            //Act
            var response = await _httpClient.GetAsync($"/documents?Number={ordernumber}");

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var responseGet = resultGet.ToObject<List<DocumentResource>>();
            Assert.Single(responseGet);
            Assert.StartsWith("/documents/", responseGet[0]?.Self);
            Assert.True(responseGet[0]?.Self.Length == 47);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetDocumentsByTags_GetMany()
        {
            //Arrange
            var ordernumber = Guid.NewGuid().ToString();

            dynamic data = new ExpandoObject();
            data.Name = "Jens Jensen";
            data.OrderNumber = ordernumber;
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var accountId = Guid.NewGuid();

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
                Input = "",
            };

            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            await _httpClient.PostAsJsonAsync("/documents", d);
            await _httpClient.PostAsJsonAsync("/documents", d);

            //Act
            var response = await _httpClient.GetAsync($"/documents?Number={ordernumber}");

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var items = resultGet.ToObject<List<DocumentResource>>();

            Assert.NotNull(items);
            Assert.Equal(2, items!.Count);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetDocumentsByTags_GetZero()
        {
            //Arrange
            var accountId = Guid.NewGuid();
            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            //Act
            var response = await _httpClient.GetAsync($"/documents?Type=NoDocument");

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var items = resultGet.ToObject<List<DocumentResource>>();
            Assert.NotNull(items);
            Assert.Empty(items);
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

            var accountId = Guid.NewGuid();

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
            var response = await _httpClient.GetAsync(itemPost?.Self);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();

            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultGet.ToObject<DocumentResource>();
            Assert.IsType<DocumentResource>(item);
            Assert.StartsWith("/documents/", item?.Self);
            Assert.True(item?.Self.Length == 47);
            Assert.EndsWith("/versions/1", item?.Version.Self);
            Assert.Contains($"/versions/", item?.Version.Self);
            Assert.True(item?.Version.Self.Length == 58);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetDocumentBy_GetLatestDocumentVersion()
        {
            //Arrange
            var ordernumber = Guid.NewGuid().ToString();

            dynamic data = new ExpandoObject();
            data.Name = "Jens Jensen";
            data.OrderNumber = ordernumber;
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var accountId = Guid.NewGuid();

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
            var response = await _httpClient.GetAsync(itemPost?.Self + "/versions/0");

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
            Assert.True(item?.Document.Self.Length == 47);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task PostDocument()
        {
            //Arrange
            var ordernumber = Guid.NewGuid().ToString();

            dynamic data = new ExpandoObject();
            data.Name = "Jens Jensen";
            data.OrderNumber = ordernumber;
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var accountId = Guid.NewGuid();

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

            //Act
            var response = await _httpClient.PostAsJsonAsync("/documents", d);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            Assert.Contains("/documents/", response.Headers.Location?.ToString());
            var stringResult = await response.Content.ReadAsStringAsync();
            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultGet.ToObject<DocumentResource>();
            Assert.IsType<DocumentResource>(item);
            Assert.StartsWith("/documents/", item?.Self);
            Assert.True(item?.Self.Length == 47);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task PutDocument()
        {
            //Arrange
            var ordernumber = Guid.NewGuid().ToString();

            dynamic data = new ExpandoObject();
            data.Name = "Jens Jensen";
            data.OrderNumber = ordernumber;
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var accountId = Guid.NewGuid();

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
                Input = "",
            };

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            //Act
            var response = await _httpClient.PostAsJsonAsync("/documents", d);

            var stringResult = await response.Content.ReadAsStringAsync();
            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultGet.ToObject<DocumentResource>();

            var templateData = new TemplateUpdateData
            {
                Input = new TemplateInput(),
                Name = $"My second template id {templateOutput.TemplateResource.Tags["id"]}"
            };

            var updateResponse = await _httpClient.PutAsJsonAsync($"{templateOutput.TemplateResource.Self}", templateData);

            var documentData = new DocumentUpdateData
            {
                Input = "",
                TemplateId = templateOutput.TemplateId,
                TemplateVersion = 2,
            };

            var updateTemplateResponse = await _httpClient.PutAsJsonAsync($"{item.Self}", documentData);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, updateResponse.StatusCode);
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

    internal class TemplateOutput
    {
        public string TemplateId { get; set; }

        public TemplateResource TemplateResource { get; set; }
    }
}