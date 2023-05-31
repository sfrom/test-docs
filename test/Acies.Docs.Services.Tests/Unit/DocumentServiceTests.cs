using Acies.Docs.Models;
using Acies.Docs.Services.Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Common.Models;
using DatabaseContext.Models;
using Logger.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Services.Tests
{
    public class DocumentServiceTests
    {
        [Fact]
        public async Task GetDocumentByTags()
        {
            //Arrange
            var tags = new Dictionary<string, string>
            {
                {"abc","Hej1"},
                {"def","Hej2"},
                {"ghi","Hej3"},
            };

            var res = new QueryResponse
            {
                Items = new List<Dictionary<string, AttributeValue>>
                {
                    new Dictionary<string, AttributeValue>
                    {
                        {"abcd",new AttributeValue("hej") },
                    },
                },
            };

            var am = new Mock<IAmazonDynamoDB>();
            am.Setup(c => c.QueryAsync(It.IsAny<QueryRequest>(), new System.Threading.CancellationToken())).ReturnsAsync(res);

            ISerializer ser = new JsonSerializerService();

            var doc = new DynamoDbDataRepositoryItem<DocumentVersion>();
            doc.Data = ser.Serialize(new DocumentVersion());

            var db = new Mock<IDynamoDBContext>();
            db.Setup(c => c.LoadAsync<DynamoDbDataRepositoryItem<DocumentVersion>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), new System.Threading.CancellationToken())).ReturnsAsync(doc);

            var docVer = new DynamoDbDataRepositoryItem<DocumentVersion>();
            docVer.Data = ser.Serialize(new DocumentVersion());

            var dbver = new Mock<IDynamoDBContext>();
            dbver.Setup(c => c.LoadAsync<DynamoDbDataRepositoryItem<DocumentVersion>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), new System.Threading.CancellationToken())).ReturnsAsync(docVer);

            var envProvider = new DefaultEnvironmentVariableProvider();
            var tenant = new TenantContext() { AccountId = "4711" };
            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = "test"
            };
            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            mock.Setup(m => m.Value).Returns(options);
            IDataRepository<DocumentVersion> r = new DynamoDbDataRepository<DocumentVersion>(am.Object, db.Object, ser, mock.Object.Value, tenant);
            IDataRepository<DocumentVersion> rv = new DynamoDbDataRepository<DocumentVersion>(am.Object, dbver.Object, ser, mock.Object.Value, tenant);
            //IDocumentService ds = new DocumentService(r, rv);

            ////Act
            //var result = await ds.GetDocumentsByTagsAsync(tags);
            ////Assert
            //Assert.Equal(doc.Data, ser.Serialize(result.First()));
        }

        [Fact]
        public void CreateDocumentOutputs_HandlebarReplaced_WhenInputPascalCasedAndOutputCamelCased()
        {
            dynamic dataInput = new ExpandoObject();
            dataInput.Name = "Ole Nielsen";
            dataInput.OrderNumber = "1212";
            dataInput.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            ISerializer ser = new JsonSerializerService();
            var input = ser.Serialize(dataInput);

            var templateVersion = new TemplateVersion()
            {
                Outputs = new List<TemplateOutputBase>() { new PdfOutput() { Name = "{{ orderNumber }}" } }
            };

            var ds = new DocumentService(null, null, null, null, null);

            var result = ds.GenerateDocumentOutputs(input, templateVersion);

            Assert.True(result.Count > 0);
            Assert.True(result[0].Name == dataInput.OrderNumber + ".pdf");
        }

        [Fact]
        public void CreateDocumentOutputs_HandlebarReplaced_WhenInputAndOutputPascalCased()
        {
            dynamic dataInput = new ExpandoObject();
            dataInput.Name = "Ole Nielsen";
            dataInput.OrderNumber = "1212";
            dataInput.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            ISerializer ser = new JsonSerializerService();
            var input = ser.Serialize(dataInput);

            var templateVersion = new TemplateVersion()
            {
                Outputs = new List<TemplateOutputBase>() { new PdfOutput() { Name = "{{ OrderNumber }}" } }
            };

            var ds = new DocumentService(null, null, null, null, null);

            var result = ds.GenerateDocumentOutputs(input, templateVersion);

            Assert.True(result.Count > 0);
            Assert.True(result[0].Name == dataInput.OrderNumber + ".pdf");
        }
    }
}
