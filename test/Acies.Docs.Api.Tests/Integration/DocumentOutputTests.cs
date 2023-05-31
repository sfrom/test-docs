using Acies.Docs.Api.Models;
using Acies.Docs.Models;
using Acies.Docs.Services;
using Amazon.S3;
using Common.Models;
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
    public class DocumentOutputTests
    {
        private readonly HttpClient _httpClient;

        public DocumentOutputTests()
        {
            Environment.SetEnvironmentVariable("DynamoDbDataRepositoryOptions__TABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
            Environment.SetEnvironmentVariable("SNS", "arn:aws:sns:eu-central-1:864358974821:sfrdevstackdocs-LocalSNS-15C72KmVXr6B");
            Environment.SetEnvironmentVariable("RESOURCE_BUCKET", "sfrdevstackdocs-teststack-generatedresourcebucket-1xe3jmzf0dxxg");
            Environment.SetEnvironmentVariable("SERVICENAME", "docs");
            var webAppFactory = new WebApplicationFactory<LocalEntryPoint>();
            _httpClient = webAppFactory.CreateDefaultClient();
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetOutputBy_GetOutputFromLatestVersion()
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
            var stringResult = await response.Content.ReadAsStringAsync();

            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultGet.ToObject<DocumentVersionResource>();

            var outputs = item.Output.ToList();
            var outputResponse = await _httpClient.GetAsync(outputs[0].Self);
            var stringOutputResult = await outputResponse.Content.ReadAsStringAsync();

            var resultOutputGet = JsonConvert.DeserializeObject<JObject>(stringOutputResult, settings)["result"];
            var itemOutput = resultOutputGet.ToObject<OutputResource>();

            //Assert

            Assert.IsType<OutputResource>(itemOutput);
            Assert.True(itemOutput.Name == "test.pdf");
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetOutputBy_GetOutputListFromLatestVersion()
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
            var stringResult = await response.Content.ReadAsStringAsync();

            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var item = resultGet.ToObject<DocumentVersionResource>();

            var outputs = item.Output.ToList();
            var outputResponse = await _httpClient.GetAsync(item.Document.Self + "/outputs");
            var stringOutputResult = await outputResponse.Content.ReadAsStringAsync();

            var resultOutputGet = JsonConvert.DeserializeObject<JObject>(stringOutputResult, settings)["result"];
            var itemOutput = resultOutputGet.ToObject<List<OutputResource>>();

            //Assert

            Assert.IsType<List<OutputResource>>(itemOutput);
            Assert.True(itemOutput.Count() == 2);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetSignedUrlForGeneratedResource()
        {
            var tenant = new TenantContext() { AccountId = "06b12620-d104-4946-8e28-b53219ded520" };
            var s3 = new AmazonS3Client();
            var outputService = new OutputService(tenant, s3);
            var urlRequest = outputService.GetPreSignedUrlRequest("test.pdf", "b92701c1-1af8-4513-9652-3ff6fed74736".ToUpper(), "866e0872-8f1d-4e64-b65f-324978961e4e".ToUpper());
            var signedUrl = outputService.GetPreSignedUrlResource(urlRequest);

            Assert.True(!string.IsNullOrEmpty(signedUrl.Url));
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
                Outputs = new List<TemplateOutputBase>() { new PdfOutput() { Name = "Test"}, new PdfOutput() { Name = "Test2" } }
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