using Amazon.DynamoDBv2.DataModel;

namespace Acies.Docs.Services.Amazon
{
    public class DynamoDbDataItem
    {
        [DynamoDBHashKey("PK")]
        public string PK { get; set; } = null!;
        [DynamoDBRangeKey("SK")]
        public string SK { get; set; } = null!;
        public string GSI1PK { get; set; }
        public string GSI1SK { get; set; }
        public string GSI2PK { get; set; }
        public string? Data { get; set; }
        public int Version { get; set; }
        public long CreatedAt { get; set; }
        public long? UpdatedAt { get; set; } = null;
        public Dictionary<string, string>? Tags { get; set; } = null;
    }
}
