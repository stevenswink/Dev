using Newtonsoft.Json;

namespace Dev.Identity.AzureTable.Model
{
    [JsonObject("identityConfiguration")]
    public class IdentityConfiguration
    {
        [JsonProperty("tablePrefix")]
        public string TablePrefix { get; set; }

        [JsonProperty("storageConnectionString")]
        public string StorageConnectionString { get; set; }

        [JsonProperty("locationMode")]
        public string LocationMode { get; set; }

        [JsonProperty("indexTableName")]
        public string IndexTableName { get; set; }

        [JsonProperty("userTableName")]
        public string UserTableName { get; set; }

        [JsonProperty("roleTableName")]
        public string RoleTableName { get; set; }
    }
}
