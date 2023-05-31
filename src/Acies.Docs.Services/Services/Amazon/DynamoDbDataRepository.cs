using Acies.Docs.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Common.Models;
using Logger.Interfaces;
using Microsoft.Extensions.Options;

namespace Acies.Docs.Services.Amazon
{
    public class DynamoDbDataRepository<T> : IDataRepository<T>
    {
        private readonly IAmazonDynamoDB _client;
        private readonly IDynamoDBContext _dynamoDBContext;
        private readonly ISerializer _serializer;
        private readonly string _partition;
        private readonly DynamoDBOperationConfig _dbConfig;
        private readonly DynamoDbDataRepositoryOptions _options;
        private readonly TenantContext _tenantContext;

        public DynamoDbDataRepository(IAmazonDynamoDB client, IDynamoDBContext dynamoDBContext, ISerializer serializer, DynamoDbDataRepositoryOptions options, TenantContext tenantContext)
        {
            _options = options;
            _tenantContext = tenantContext;
            _client = client;
            _dynamoDBContext = dynamoDBContext;
            _serializer = serializer;
            _partition = $"#{typeof(T).Name}";

            string dataTable = _options.Table;

            if (string.IsNullOrEmpty(dataTable)) throw new Exception("DataTable:name not found in environment variable");

            _dbConfig = new DynamoDBOperationConfig
            {
                OverrideTableName = dataTable
            };
        }

        public async Task<T?> GetAsync(string key, int version, ConcurrencyToken? concurrencyToken = null)
        {
            var r = await _dynamoDBContext.LoadAsync<DynamoDbDataRepositoryItem<T>>(_tenantContext.AccountId + _partition, JoinKeyVersion(key, version), _dbConfig);
            if (r == null) return default(T);
            return _serializer.Deserialize<T>(r.Data);
        }

        public async Task<IEnumerable<string>> GetKeysByTagsAsync(IDictionary<string, string> tags)
        {
            var filterExpression = tags.Aggregate("", (a, b) => a + $" AND Tags.#{b.Key}=:{b.Key}")[5..];
            var expressionAttributeNames = new Dictionary<string, string>();
            var expressionAttributeValues = new Dictionary<string, AttributeValue>();
            foreach (var tag in tags)
            {
                expressionAttributeNames.Add("#" + tag.Key, tag.Key);
                expressionAttributeValues.Add(":" + tag.Key, new AttributeValue { S = tag.Value });
            };
            expressionAttributeNames.Add("#PK", "PK");
            expressionAttributeValues.Add(":pk", new AttributeValue { S = _tenantContext.AccountId + _partition });

            QueryRequest request = new QueryRequest
            {
                TableName = _dbConfig.OverrideTableName,
                KeyConditionExpression = "#PK=:pk",
                FilterExpression = filterExpression,
                ExpressionAttributeValues = expressionAttributeValues,
                ExpressionAttributeNames = expressionAttributeNames,
                ProjectionExpression = "SK",
            };

            var r = await _client.QueryAsync(request);
            return r.Items.Select(x => x.First().Value.S);
        }

        public async Task<int> GetLatestVersionAsync(string key)
        {
            var expressionAttributeNames = new Dictionary<string, string>
            {
                { "#PK", "PK" },
                { "#SK", "SK" },
            };

            var expressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue { S = _tenantContext.AccountId + _partition }},
                { ":sk", new AttributeValue { S = $"{key}/" }},
            };

            QueryRequest request = new QueryRequest
            {
                TableName = _dbConfig.OverrideTableName,
                KeyConditionExpression = "#PK=:pk AND begins_with(#SK,:sk)",
                ExpressionAttributeValues = expressionAttributeValues,
                ExpressionAttributeNames = expressionAttributeNames,
                ProjectionExpression = "SK",
            };

            var r = await _client.QueryAsync(request);
            return int.Parse(r.Items.Select(x => x.First().Value.S).OrderBy(c => c).Last().Split('/').Last());
        }

        public async Task SaveAsync(T data, string key, int version, ConcurrencyToken? concurrencyToken = null)
        {
            var taggedItem = data as ITaggedItem;
            var dynamoItem = new DynamoDbDataRepositoryItem<T>
            {
                Data = _serializer.Serialize(data),
                PK = _tenantContext.AccountId + _partition,
                SK = JoinKeyVersion(key, version),
                ConcurrencyVersion = version,
                Tags = taggedItem?.Tags,
            };

            await _dynamoDBContext.SaveAsync(dynamoItem, _dbConfig);
        }

        private string JoinKeyVersion(string key, int version)
        {
            return key + "/" + version.ToString().PadLeft(10, '0');
        }
    }
}