using Acies.Docs.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using Acies.Docs.Services.Amazon;
using Logger.Services;
using Moq;
using Microsoft.Extensions.Options;
using Common.Models;

namespace Acies.Docs.Services.Tests
{
    public class DataClass : ITaggedItem
    {
        public string PK { get; set; } = null!;
        public string SK { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Number { get; set; }
        public string Description { get; set; } = null!;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class DataRepositoryTests
    {
        public DataRepositoryTests()
        {
            Environment.SetEnvironmentVariable("DBTABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
        }
        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task Concurrency_Fail()
        {
            ////Arrange
            //const string key = "30bb91c5-459b-4283-8c45-bdd2f7b1140e";
            //var a = new AmazonDynamoDBClient();
            //var c = new DynamoDBContext(a);
            //ISerializer ser = new JsonSerializerService();
            //var tenant = new TenantContext() { AccountId = "4711" };
            //var options = new DynamoDbDataRepositoryOptions()
            //{
            //    Table = Environment.GetEnvironmentVariable("DBTABLE")
            //};
            //var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            //mock.Setup(m => m.Value).Returns(options);
            //var s = new DynamoDbDataRepository<Document>(a, c, ser, mock.Object.Value, tenant);
            //var ct = new ConcurrencyToken();
            //var doc = await s.GetAsync(key, 1, ct);
            //await s.SaveAsync(doc, key, 1, ct);

            ////Act
            //Func<Task> act = () => s.SaveAsync(doc, key, 1, ct);

            ////Assert
            //await Assert.ThrowsAsync<ConditionalCheckFailedException>(act);
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task SaveDataInDynamoDb()
        {
            var ac = new AmazonDynamoDBClient();
            var db = new DynamoDBContext(ac);
            ISerializer ser = new JsonSerializerService();
            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = Environment.GetEnvironmentVariable("DBTABLE")
            };
            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            mock.Setup(m => m.Value).Returns(options);
            var tenant = new TenantContext() { AccountId = "4711" };
            var s = new DynamoDbDataRepository<DataClass>(ac, db, ser, mock.Object.Value, tenant);

            var d = new DataClass
            {
                PK = typeof(DataClass).Name + "/123",
                SK = "00000001/" + Guid.NewGuid().ToString(),
                Name = "YY",
                Number = 40,
                Description = "xyz"
            };
            d.Tags.Add("ItemType", "Order");
            d.Tags.Add("Number", "123");
            d.Tags.Add("OrderType", "Single");
            try
            {
                await s.SaveAsync(d, "abc", 1);
            }
            catch (Exception ex)
            {
                var e = ex.ToString();
            }
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetByTags()
        {
            var ac = new AmazonDynamoDBClient();
            var db = new DynamoDBContext(ac);
            ISerializer ser = new JsonSerializerService();
            var t = new Dictionary<string, string>
            {
                { "ItemType", "Order"},
                { "Number","123"},
            };
            var tenant = new TenantContext() { AccountId = "4711" };
            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = Environment.GetEnvironmentVariable("DBTABLE")
            };
            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            mock.Setup(m => m.Value).Returns(options);
            var s = new DynamoDbDataRepository<DataClass>(ac, db, ser, mock.Object.Value, tenant);
            try
            {
                var r = await s.GetKeysByTagsAsync(t);
                var dataList = new List<DataClass>();
                foreach (var key in r)
                {
                    var data = await s.GetAsync(key.Split('/').First(), 1);
                    if (data != null)
                    {
                        dataList.Add(data);
                    }
                }
                Assert.True(dataList.Count == 1, "One item should be found in the database");
            }
            catch (Exception ex)
            {
                var e = ex.ToString();
            }
        }
    }
}
