using Acies.Docs.Api.Models;
using Acies.Docs.Models;
using Acies.Docs.Services.Amazon;
using Acies.Docs.Services.Repositories;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Common.Models;
using DatabaseContext.Models;
using Logger.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Services.Tests
{
    public class TemplateTests
    {
        public TemplateTests()
        {
            Environment.SetEnvironmentVariable("DBTABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task CreateTemplate_Ok()
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
            var dbs = new TemplateRepository(a, c, ser, mock.Object.Value, tenant);

            ITemplateService s = new TemplateService(dbs);

            var data = new TemplateCreateData
            {
                Name = "My template",
                Input = new TemplateInput(),
                Outputs = new List<TemplateOutputBase>
                {
                    new StaticOutput
                    {
                        Name="Pdf output",
                        Asset=new Asset
                        {
                            Path="/docs/terms.pdf",
                        },
                        Tags = new Dictionary<string, string>
                        {
                            {"Type","Order" },
                            {"Subtype","Terms" },
                        },
                    },
                    new PdfOutput
                    {
                        Name="Pdf output",
                        Layout = new Layout
                        {
                            Format="A3",
                            Margins=new Margins
                            {
                                Top=10,
                                Right=11,
                                Bottom=12,
                                Left=13,
                            },
                            Header = new Models.TemplateRef
                            {
                                Content="<div>Header</div>",
                            },
                            Body = new Models.TemplateRef
                            {
                                Content ="<html>Myhtml</html>",
                            },
                            Footer = new Models.TemplateRef
                            {
                                Content= "<div>Footer</div>",
                            },
                            Assets=new List<Asset>
                            {
                               new Asset
                               {
                                   Path="/images/acies-logo.png",
                               },
                               new Asset
                               {
                                   Path="/docs/terms",
                               }
                            },
                        },
                        Tags=new Dictionary<string, string>
                        {
                            {"Type", "Order" },
                        },
                    },
                },
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1216" },
                },
            };

            //Act
            var r = await s.CreateTemplateAsync(data);

            //Assert
            Assert.NotNull(r);
            Assert.IsType<Template>(r);
            Assert.NotNull(r.Id);
            Assert.NotEmpty(r.Id);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task UpdateTemplate_Ok()
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
            var dbs = new TemplateRepository(a, c, ser, mock.Object.Value, tenant);

            ITemplateService s = new TemplateService(dbs);

            var data = new TemplateCreateData
            {
                Name = "My template",
                Input = new TemplateInput(),
                Outputs = new List<TemplateOutputBase>
                {
                    new StaticOutput
                    {
                        Name="Pdf output",
                        Asset=new Asset
                        {
                            Path="/docs/terms.pdf",
                        },
                        Tags = new Dictionary<string, string>
                        {
                            {"Type","Order" },
                            {"Subtype","Terms" },
                        },
                    },
                    new PdfOutput
                    {
                        Name="Pdf output",
                        Layout = new Layout
                        {
                            Format="A3",
                            Margins=new Margins
                            {
                                Top=10,
                                Right=11,
                                Bottom=12,
                                Left=13,
                            },
                            Header = new Models.TemplateRef
                            {
                                Content="<div>Header</div>",
                            },
                            Body = new Models.TemplateRef
                            {
                                Content ="<html>Myhtml</html>",
                            },
                            Footer = new Models.TemplateRef
                            {
                                Content= "<div>Footer</div>",
                            },
                            Assets=new List<Asset>
                            {
                               new Asset
                               {
                                   Path="/images/acies-logo.png",
                               },
                               new Asset
                               {
                                   Path="/docs/terms.pdf",
                               }
                            },
                        },
                        Tags=new Dictionary<string, string>
                        {
                            {"Type", "Order" },
                        },
                    },
                },
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1216" },
                },
            };

            var dataUpdate = new TemplateUpdateData
            {
                Name = "My template",
                Input = new TemplateInput(),
            };

            //Act
            var r = await s.CreateTemplateAsync(data);
            var u = await s.UpdateTemplateAsync(r.Id, dataUpdate);

            //Assert
            Assert.NotNull(u);
            Assert.IsType<Template>(u);
            Assert.NotNull(u.Id);
            Assert.NotEmpty(u.Id);
        }
    }
}
