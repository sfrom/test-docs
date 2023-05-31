using Acies.Docs.Api.Models;
using Acies.Docs.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
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
    public class TemplateTests
    {
        private readonly HttpClient _httpClient;

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public TemplateTests()
        {
            Environment.SetEnvironmentVariable("DynamoDbDataRepositoryOptions__TABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
            Environment.SetEnvironmentVariable("SERVICENAME", "docs");
                        var webAppFactory = new WebApplicationFactory<LocalEntryPoint>();
            _httpClient = webAppFactory.CreateDefaultClient();
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetTemplatesByTags_GetOne()
        {
            var id = Guid.NewGuid().ToString();
            //Arrange
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
            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            var rt = await _httpClient.PostAsJsonAsync("/templates", t);

            //Act
            var response = await _httpClient.GetAsync($"/templates?Id={id}");

            //Assert
            var stringResultSave = await rt.Content.ReadAsStringAsync();
            var resultSave = JsonConvert.DeserializeObject<JObject>(stringResultSave, settings)["result"];
            var item = resultSave.ToObject<TemplateResource>();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();
            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var items = resultGet.ToObject<List<TemplateResource>>();

            Assert.Single<TemplateResource>(items);
            Assert.Equal($"My first template id {id}", items?.First().Name);
            Assert.StartsWith("/templates/", items[0]?.Self);
            Assert.True(items[0]?.Self.Length == 47);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetTemplatesByTags_GetMany()
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

            var t2 = new TemplateCreateData
            {
                Name = $"My second template id {id}",
                Version = 1,
                Input = new TemplateInput(),
                Tags = new Dictionary<string, string>
                {
                    { "Type","Order" },
                    { "Id",id },
                },
            };

            var accountId = Guid.NewGuid();
            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            await _httpClient.PostAsJsonAsync("/templates", t);
            await _httpClient.PostAsJsonAsync("/templates", t2);

            //Act
            var response = await _httpClient.GetAsync($"/templates?Id={id}");

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();
            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var items = resultGet.ToObject<List<TemplateResource>>();
            Assert.NotNull(items);
            Assert.Equal(2, items!.Count);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetTemplatesByTags_GetZero()
        {
            //Arrange
            var accountId = Guid.NewGuid();
            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            //Act
            var response = await _httpClient.GetAsync($"/templates?Type=NoDocument");

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();
            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var items = resultGet.ToObject<List<TemplateResource>>();
            Assert.NotNull(items);
            Assert.Empty(items);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetTemplateBy_GetLatestTemplate()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var t = new TemplateCreateData
            {
                Name = $"My first template id {id}",
                Version = 1,
                Input = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': false, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 1}}}" },
                Tags = new Dictionary<string, string>
                {
                    { "Type","Order" },
                    { "Id",id },
                },
            };

            var accountId = Guid.NewGuid();
            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            var responsePost = await _httpClient.PostAsJsonAsync("/templates", t);
            var stringResultPost = await responsePost.Content.ReadAsStringAsync();
            var resultPost = JsonConvert.DeserializeObject<JObject>(stringResultPost, settings)["result"];
            var itemPost = resultPost.ToObject<TemplateResource>();

            //Act
            var response = await _httpClient.GetAsync(itemPost?.Self + "/versions/" + 0);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();
            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultGet.ToObject<TemplateVersionResource>();
            Assert.IsType<TemplateVersionResource>(item);
            Assert.StartsWith("/templates/", item?.Self);
            Assert.True(item?.Self.Length == 58);
            Assert.EndsWith("/versions/1", item?.Self);
            Assert.StartsWith("/templates/", item?.Template.Self);
            Assert.True(item?.Template.Self.Length == 47);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetTemplateBy_GetTemplateById()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var t = new TemplateCreateData
            {
                Name = $"My first template id {id}",
                Version = 1,
                Input = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': false, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 1}}}" },
                Tags = new Dictionary<string, string>
                {
                    { "Type","Order" },
                    { "Id",id },
                },
            };

            var accountId = Guid.NewGuid();
            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            var responsePost = await _httpClient.PostAsJsonAsync("/templates", t);
            var stringResultPost = await responsePost.Content.ReadAsStringAsync();
            var resultPost = JsonConvert.DeserializeObject<JObject>(stringResultPost, settings)["result"];
            var itemPost = resultPost.ToObject<TemplateResource>();

            //Act
            var response = await _httpClient.GetAsync(itemPost?.Self);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();
            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultGet.ToObject<TemplateResource>();
            Assert.IsType<TemplateResource>(item);
            Assert.StartsWith("/templates/", item?.Self);
            Assert.True(item?.Self.Length == 47);
            Assert.Contains("/versions/", item?.Version.Self);
            Assert.True(item?.Version.Self.Length == 58);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task PostTemplate()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            //var createData = @"{'name': 'My first template id " + id + @"', 'version': 1, 'input': {'validation': {'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': false, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 1}}}}, 'tags': {'type': 'Order', 'id': '" + id + @"'} }";
            var t = new TemplateCreateData
            {
                Name = $"My first template id {id}",
                Version = 1,
                Input = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': false, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 1}}}" },
                Tags = new Dictionary<string, string>
                {
                    { "Type","Order" },
                    { "Id",id },
                },
            };
            var accountId = Guid.NewGuid();
            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            //Act
            var response = await _httpClient.PostAsJsonAsync("/templates", t);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var stringResult = await response.Content.ReadAsStringAsync();
            var resultPost = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultPost.ToObject<TemplateResource>();
            Assert.IsType<TemplateResource>(item);
            Assert.StartsWith("/templates/", item?.Self);
            Assert.True(item?.Self.Length == 47);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task PutDocument()
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
            _httpClient.DefaultRequestHeaders.Add("x-account-id", accountId.ToString());

            //Act
            var response = await _httpClient.PostAsJsonAsync("/templates", t);

            var stringResult = await response.Content.ReadAsStringAsync();
            var resultPost = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultPost.ToObject<TemplateResource>();

            var templateData = new TemplateUpdateData
            {
                Input = new TemplateInput(),
                Name = $"My second template id {id}"
            };

            var updateResponse = await _httpClient.PutAsJsonAsync($"{item.Self}", templateData);

            var stringResultUpdate = await updateResponse.Content.ReadAsStringAsync();
            var resultPostUpdate = JsonConvert.DeserializeObject<JObject>(stringResultUpdate, settings)["result"];
            var itemUpdate = resultPostUpdate.ToObject<TemplateResource>();

            //Act
            var responseRead = await _httpClient.GetAsync(itemUpdate?.Version?.Self);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, updateResponse.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, responseRead.StatusCode);
            var stringResultRead = await responseRead.Content.ReadAsStringAsync();
            var resultRead = JsonConvert.DeserializeObject<JObject>(stringResultRead, settings)["result"];
            var itemRead = resultRead.ToObject<TemplateVersionResource>();
            Assert.IsType<Api.Models.TemplateVersionResource>(itemRead);
            Assert.EndsWith("/versions/2", itemRead?.Self);
            Assert.True(itemRead.Name == $"My second template id {id}");
        }
    }
}