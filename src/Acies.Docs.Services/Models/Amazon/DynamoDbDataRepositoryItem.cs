using Amazon.DynamoDBv2.DataModel;

namespace Acies.Docs.Services.Amazon
{
    public class DynamoDbDataRepositoryItem<T>
    {
        [DynamoDBHashKey("PK")]
        public string PK { get; set; } = null!;
        [DynamoDBRangeKey("SK")]
        public string SK { get; set; } = null!;
        [DynamoDBVersion]
        public int? ConcurrencyVersion { get; set; }
        public string Data { get; set; } = null!;
        public Dictionary<string, string>? Tags { get; set; } = new Dictionary<string, string>();
    }
}
