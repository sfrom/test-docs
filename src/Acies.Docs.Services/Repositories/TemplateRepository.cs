using Acies.Docs.Models;
using Acies.Docs.Services.Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using DocModel = Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Common.Models;

namespace Acies.Docs.Services.Repositories
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly IAmazonDynamoDB _client;
        private readonly IDynamoDBContext _dynamoDBContext;
        private readonly ISerializer _serializer;
        private readonly DynamoDBOperationConfig _dbConfig;
        private readonly DynamoDbDataRepositoryOptions _options;
        private readonly TenantContext _tenantContext;

        public TemplateRepository(IAmazonDynamoDB client, IDynamoDBContext dynamoDBContext, ISerializer serializer, DynamoDbDataRepositoryOptions options, TenantContext tenantContext)
        {
            _options = options;
            _tenantContext = tenantContext;
            _client = client;
            _dynamoDBContext = dynamoDBContext;
            _serializer = serializer;

            string dataTable = _options.Table;

            if (string.IsNullOrEmpty(dataTable)) throw new Exception("DataTable:name not found in environment variable");

            _dbConfig = new DynamoDBOperationConfig
            {
                OverrideTableName = dataTable
            };
        }

        public async Task<Template?> GetAsync(string key)
        {
            var pk = "ACCOUNT#" + _tenantContext.AccountId.ToUpper();
            var sk = ("XMETATEMP#" + key).ToUpper();
            DynamoDbDataItem r = await _dynamoDBContext.LoadAsync<DynamoDbDataItem>(pk, sk, _dbConfig);
            if (r == null) return default;
            return _serializer.Deserialize<Template>(r.Data);
        }

        public async Task<TemplateVersion?> GetVersionAsync(string key, int version)
        {
            var pk = ("TEMPVERSION#" + JoinKeyVersion(key, version)).ToUpper();
            if(_dynamoDBContext == null)
            {
                Console.WriteLine("dynamodbcontext null!!");
            }
            try
            {
                DynamoDbDataItem r = await _dynamoDBContext.LoadAsync<DynamoDbDataItem>(pk, pk, _dbConfig);
                if (r == null)
                {
                    return default;
                }
                else
                {
                    return _serializer.Deserialize<TemplateVersion>(r.Data);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            
        }

        public async Task<IEnumerable<Template>?> GetBatchAsync(IEnumerable<string> keys)
        {
            var batchget = _dynamoDBContext.CreateBatchGet<DynamoDbDataItem>(_dbConfig);
            foreach (var k in keys)
            {
                batchget.AddKey(("ACCOUNT#" + _tenantContext.AccountId).ToUpper(), ("XMETATEMP#" + k).ToUpper());
            }
            await batchget.ExecuteAsync();
            var results = batchget.Results;
            var documents = new List<Template>();
            foreach (var r in results)
            {
                var doc = _serializer.Deserialize<Template>(r.Data);
                documents.Add(doc);
            }
            return documents;
        }

        public async Task<TemplateVersion?> GetLatestAsync(string key)
        {
            var pk = ("ACCOUNT#" + _tenantContext.AccountId + "#XMETATEMP#" + key).ToUpper();
            var expressionAttributeNames = new Dictionary<string, string>();
            var expressionAttributeValues = new Dictionary<string, AttributeValue>();

            expressionAttributeNames.Add("#PK", "GSI1PK");
            expressionAttributeValues.Add(":pk", new AttributeValue(pk));

            QueryRequest request = new QueryRequest
            {
                TableName = _dbConfig.OverrideTableName,
                IndexName = "Versions",
                ScanIndexForward = false,
                Limit = 2,
                KeyConditionExpression = "#PK=:pk",
                ExpressionAttributeNames = expressionAttributeNames,
                ExpressionAttributeValues = expressionAttributeValues
            };
            var r = await _client.QueryAsync(request);
            var tempVer = DocModel.Document.FromAttributeMap(r.Items[1]);
            var tempVerData = _dynamoDBContext.FromDocument<DynamoDbDataItem>(tempVer);
            return _serializer.Deserialize<TemplateVersion>(tempVerData.Data);

        }

        public async Task<IEnumerable<string>> GetKeysByTagsAsync(IDictionary<string, string> tags)
        {
            var lowerTags = tags != null ? tags.ToDictionary(kv => kv.Key.ToLower(), kv => kv.Value) : new Dictionary<string, string>();
            var filterExpression = lowerTags?.Aggregate("", (a, b) => a + $" AND Tags.#{b.Key}=:{b.Key}")[5..];
            var expressionAttributeNames = new Dictionary<string, string>();
            var expressionAttributeValues = new Dictionary<string, AttributeValue>();
            foreach (var tag in lowerTags)
            {
                expressionAttributeNames.Add("#" + tag.Key, tag.Key);
                expressionAttributeValues.Add(":" + tag.Key, new AttributeValue { S = tag.Value });
            };
            expressionAttributeNames.Add("#PK", "PK");
            expressionAttributeValues.Add(":pk", new AttributeValue { S = "ACCOUNT#" + _tenantContext.AccountId.ToUpper() });

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
            return r.Items.Count() > 0 ? r.Items.Select(x => x.First().Value.S.Split("#")[1]) : new List<string>();
        }

        //public async Task<int> GetLatestVersionAsync(string key)
        //{
        //    var expressionAttributeNames = new Dictionary<string, string>
        //    {
        //        { "#PK", "PK" },
        //        { "#SK", "SK" },
        //    };

        //    var expressionAttributeValues = new Dictionary<string, AttributeValue>
        //    {
        //        { ":pk", new AttributeValue { S = _tenantContext.AccountId + _partition }},
        //        { ":sk", new AttributeValue { S = $"{key}/" }},
        //    };

        //    QueryRequest request = new QueryRequest
        //    {
        //        TableName = _dbConfig.OverrideTableName,
        //        KeyConditionExpression = "#PK=:pk AND begins_with(#SK,:sk)",
        //        ExpressionAttributeValues = expressionAttributeValues,
        //        ExpressionAttributeNames = expressionAttributeNames,
        //        ProjectionExpression = "SK",
        //    };

        //    var r = await _client.QueryAsync(request);
        //    return int.Parse(r.Items.Select(x => x.First().Value.S).OrderBy(c => c).Last().Split('/').Last());
        //}

        public async Task SaveAsync(Template data, string key, int version)
        {
            var taggedItem = data as ITaggedItem;
            var lowerTags = taggedItem?.Tags?.ToDictionary(kv => kv.Key.ToLower(), kv => kv.Value);
            var pk = "ACCOUNT#" + _tenantContext.AccountId.ToUpper();
            var sk = ("XMETATEMP#" + key).ToUpper();

            var dynamoItem = new DynamoDbDataItem()
            {
                PK = pk,
                SK = sk,
                GSI1PK = pk + "#" + sk,
                GSI1SK = sk,
                GSI2PK = pk,
                Data = _serializer.Serialize(data),
                Version = data.Version,
                CreatedAt = data.CreatedAt,
                UpdatedAt = data.UpdatedAt,
                Tags = lowerTags,
            };

            await _dynamoDBContext.SaveAsync(dynamoItem, _dbConfig);
        }

        public async Task SaveVersionAsync(TemplateVersion data, string key, int version)
        {
            var pk = ("TEMPVERSION#" + JoinKeyVersion(key, version)).ToUpper();
            var meta = ("XMETATEMP#" + key).ToUpper();
            var acc = "ACCOUNT#" + _tenantContext.AccountId.ToUpper();

            var dynamoItem = new DynamoDbDataItem()
            {
                PK = pk,
                SK = pk,
                GSI1PK = acc + "#" + meta,
                GSI1SK = pk,
                GSI2PK = acc,
                Version = data.Version,
                Data = _serializer.Serialize(data),
                CreatedAt = data.CreatedAt,
            };

            await _dynamoDBContext.SaveAsync(dynamoItem, _dbConfig);
        }

        private string JoinKeyVersion(string key, int version)
        {
            return key + "/" + version.ToString().PadLeft(10, '0');
        }
    }
}