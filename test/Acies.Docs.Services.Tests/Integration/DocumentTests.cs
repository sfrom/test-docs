using Acies.Docs.Models;
using Acies.Docs.Models.Interfaces;
using Acies.Docs.Services.Amazon;
using Acies.Docs.Services.Repositories;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Common.Models;
using DatabaseContext.Models;
using Logger.Services;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Services.Tests
{
    public class DocumentTests
    {
        public DocumentTests()
        {
            Environment.SetEnvironmentVariable("DBTABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetDocumentByTags_Ok()
        {
            //Arrange
            IAmazonDynamoDB a = new AmazonDynamoDBClient();
            IDynamoDBContext c = new DynamoDBContext(a);
            ISerializer ser = new JsonSerializerService();
            var tenant = new TenantContext() { AccountId = "4711" };
            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = Environment.GetEnvironmentVariable("DBTABLE")
            };
            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            mock.Setup(m => m.Value).Returns(options);

            var mockSns = new Mock<ISNSMessageService>();

            var mockTemplateRepo = new Mock<ITemplateRepository>();
            mockTemplateRepo.Setup(m => m.GetVersionAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new TemplateVersion());

            var mockValidationService = new Mock<IValidationService>();
            mockValidationService.Setup(m => m.ValidateJson(It.IsAny<string>(), It.IsAny<string>())).Returns(new ValidationResponse());

            var dbs = new DocumentRepository(a, c, ser, mock.Object.Value, tenant);

            dynamic data = new ExpandoObject();
            data.Name = "Ole Nielsen";
            data.OrderNumber = "1212";
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var documentData = new DocumentCreateData
            {
                Input = ser.Serialize(data),
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1217" },
                },
                TemplateId = "23949b37-1c43-4f2d-bf18-d4f84b29f53d",
                TemplateVersion = 2,
            };

            IDocumentService s = new DocumentService(dbs, mockTemplateRepo.Object, mockSns.Object, tenant, mockValidationService.Object);

            //Act
            var created = await s.CreateDocumentAsync(documentData);
            var read = await s.GetDocumentsByTagsAsync(new Dictionary<string, string> { { "Number", "1217" } });

            //Assert
            Assert.NotNull(read);
            Assert.Equal("1217", read.First().Tags["number"]);
            mockSns.Verify(x => x.UpdateStatusAsync(It.IsAny<DocumentVersion>(), It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()), Times.Once);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task UpdateDocument_Ok()
        {
            //Arrange
            IAmazonDynamoDB a = new AmazonDynamoDBClient();
            IDynamoDBContext c = new DynamoDBContext(a);
            ISerializer ser = new JsonSerializerService();
            var envProvider = new DefaultEnvironmentVariableProvider();
            var tenant = new TenantContext() { AccountId = "4711" };
            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = Environment.GetEnvironmentVariable("DBTABLE")
            };
            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            mock.Setup(m => m.Value).Returns(options);
            var mockSns = new Mock<ISNSMessageService>();

            var mockTemplateRepo = new Mock<ITemplateRepository>();
            mockTemplateRepo.Setup(m => m.GetVersionAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new TemplateVersion());
            var mockValidationService = new Mock<IValidationService>();
            mockValidationService.Setup(m => m.ValidateJson(It.IsAny<string>(), It.IsAny<string>())).Returns(new ValidationResponse());

            var dbs = new DocumentRepository(a, c, ser, mock.Object.Value, tenant);

            dynamic dataCreate = new ExpandoObject();
            dataCreate.Name = "Ole Nielsen";
            dataCreate.OrderNumber = "1212";
            dataCreate.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var documentDataCreate = new DocumentCreateData
            {
                Input = ser.Serialize(dataCreate),
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1210" },
                },
                TemplateId = "23949b37-1c43-4f2d-bf18-d4f84b29f53d",
                TemplateVersion = 2,
            };

            dynamic data = new ExpandoObject();
            data.Name = "Ole Nielsen Jensen";
            data.OrderNumber = "1212";
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var documentData = new DocumentUpdateData
            {
                Input = ser.Serialize(data),
                TemplateId = "23949b37-1c43-4f2d-bf18-d4f84b29f53d",
                TemplateVersion = 3,
            };
            IDocumentService s = new DocumentService(dbs, mockTemplateRepo.Object, mockSns.Object, tenant, mockValidationService.Object);

            //Act
            var created = await s.CreateDocumentAsync(documentDataCreate);
            var r = await s.UpdateDocumentAsync(created.Document.Id, documentData);

            //Assert
            Assert.NotNull(r);
            Assert.True(r.Document.Version == 2, "Latest version should be 2");
            mockSns.Verify(x => x.UpdateStatusAsync(It.IsAny<DocumentVersion>(), It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()), Times.Once);

        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetLatestDocumentVersion_Ok()
        {
            //Arrange
            IAmazonDynamoDB a = new AmazonDynamoDBClient();
            IDynamoDBContext c = new DynamoDBContext(a);
            ISerializer ser = new JsonSerializerService();
            var envProvider = new DefaultEnvironmentVariableProvider();
            var tenant = new TenantContext() { AccountId = "4711" };
            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = Environment.GetEnvironmentVariable("DBTABLE")
            };
            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            mock.Setup(m => m.Value).Returns(options);
            var mockSns = new Mock<ISNSMessageService>();

            var mockTemplateRepo = new Mock<ITemplateRepository>();
            mockTemplateRepo.Setup(m => m.GetVersionAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new TemplateVersion());
            var mockValidationService = new Mock<IValidationService>();
            mockValidationService.Setup(m => m.ValidateJson(It.IsAny<string>(), It.IsAny<string>())).Returns(new ValidationResponse());

            var dbs = new DocumentRepository(a, c, ser, mock.Object.Value, tenant);

            dynamic dataCreate = new ExpandoObject();
            dataCreate.Name = "Ole Nielsen";
            dataCreate.OrderNumber = "1212";
            dataCreate.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var documentDataCreate = new DocumentCreateData
            {
                Input = ser.Serialize(dataCreate),
                Tags = new Dictionary<string, string>   
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1210" },
                },
                TemplateId = "23949b37-1c43-4f2d-bf18-d4f84b29f53d",
                TemplateVersion = 2,
            };

            dynamic data = new ExpandoObject();
            data.Name = "Ole Nielsen Jensen";
            data.OrderNumber = "1212";
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var documentData = new DocumentUpdateData
            {
                Input = ser.Serialize(data),
                TemplateId = "23949b37-1c43-4f2d-bf18-d4f84b29f53d",
                TemplateVersion = 3,
            };
            IDocumentService s = new DocumentService(dbs, mockTemplateRepo.Object, mockSns.Object, tenant, mockValidationService.Object);

            //Act
            var created = await s.CreateDocumentAsync(documentDataCreate);
            var updated = await s.UpdateDocumentAsync(created.Document.Id, documentData);
            var r = await s.GetLatestVersionAsync(updated.Document.Id);

            //Assert
            Assert.NotNull(r);
            Assert.True(r.Version == 2, "Latest version should be 2");
            Assert.True(!string.IsNullOrWhiteSpace(r.Input), "Input should not be empty on document version");
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetOldDocumentVersion_Ok()
        {
            //Arrange
            IAmazonDynamoDB a = new AmazonDynamoDBClient();
            IDynamoDBContext c = new DynamoDBContext(a);
            ISerializer ser = new JsonSerializerService();
            var envProvider = new DefaultEnvironmentVariableProvider();
            var tenant = new TenantContext() { AccountId = "4711" };
            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = Environment.GetEnvironmentVariable("DBTABLE")
            };
            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            mock.Setup(m => m.Value).Returns(options);
            var mockSns = new Mock<ISNSMessageService>();

            var mockTemplateRepo = new Mock<ITemplateRepository>();
            mockTemplateRepo.Setup(m => m.GetVersionAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new TemplateVersion());
            var mockValidationService = new Mock<IValidationService>();
            mockValidationService.Setup(m => m.ValidateJson(It.IsAny<string>(), It.IsAny<string>())).Returns(new ValidationResponse());

            var dbs = new DocumentRepository(a, c, ser, mock.Object.Value, tenant);

            dynamic dataCreate = new ExpandoObject();
            dataCreate.Name = "Ole Nielsen";
            dataCreate.OrderNumber = "1212";
            dataCreate.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var documentDataCreate = new DocumentCreateData
            {
                Input = ser.Serialize(dataCreate),
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1210" },
                },
                TemplateId = "23949b37-1c43-4f2d-bf18-d4f84b29f53d",
                TemplateVersion = 2,
            };

            dynamic data = new ExpandoObject();
            data.Name = "Ole Nielsen Jensen";
            data.OrderNumber = "1212";
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var documentData = new DocumentUpdateData
            {
                Input = ser.Serialize(data),
                TemplateId = "23949b37-1c43-4f2d-bf18-d4f84b29f53d",
                TemplateVersion = 3,
            };
            IDocumentService s = new DocumentService(dbs, mockTemplateRepo.Object, mockSns.Object, tenant, mockValidationService.Object);

            //Act
            var created = await s.CreateDocumentAsync(documentDataCreate);
            var updated = await s.UpdateDocumentAsync(created.Document.Id, documentData);
            var r = await s.GetDocumentVersionByKeyAsync(updated.Document.Id, 1);

            //Assert
            Assert.NotNull(r);
            Assert.True(r.Version == 1, "Version should be 1");
            Assert.True(r.TemplateVersion == 2, "Template version should be 2");
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task CreateDocument_Ok()
        {
            //Arrange
            IAmazonDynamoDB a = new AmazonDynamoDBClient();
            IDynamoDBContext c = new DynamoDBContext(a);
            ISerializer ser = new JsonSerializerService();
            var envProvider = new DefaultEnvironmentVariableProvider();
            var tenant = new TenantContext() { AccountId = "4711" };
            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = Environment.GetEnvironmentVariable("DBTABLE")
            };
            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            mock.Setup(m => m.Value).Returns(options);
            var mockSns = new Mock<ISNSMessageService>();

            var mockTemplateRepo = new Mock<ITemplateRepository>();
            mockTemplateRepo.Setup(m => m.GetVersionAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new TemplateVersion());
            var mockValidationService = new Mock<IValidationService>();
            mockValidationService.Setup(m => m.ValidateJson(It.IsAny<string>(), It.IsAny<string>())).Returns(new ValidationResponse());

            var dbs = new DocumentRepository(a, c, ser, mock.Object.Value, tenant);

            dynamic data = new ExpandoObject();
            data.Name = "Ole Nielsen";
            data.OrderNumber = "1212";
            data.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var documentData = new DocumentCreateData
            {
                Input = ser.Serialize(data),
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1220" },
                },
                TemplateId = "23949b37-1c43-4f2d-bf18-d4f84b29f53d",
                TemplateVersion = 2,
            };
            IDocumentService s = new DocumentService(dbs, mockTemplateRepo.Object, mockSns.Object, tenant, mockValidationService.Object);

            //Act
            var r = await s.CreateDocumentAsync(documentData);

            //Assert
            Assert.NotNull(r);
            Assert.IsType<Document>(r.Document);
            Assert.NotNull(r.Document.Id);
            Assert.NotEmpty(r.Document.Id);
        }
    }
}
