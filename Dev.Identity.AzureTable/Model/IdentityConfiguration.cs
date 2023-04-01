namespace Dev.Identity.AzureTable.Model
{
    public class IdentityConfiguration
    {
        public string TablePrefix { get; set; }

        public string StorageConnectionString { get; set; }

        public string IndexTableName { get; set; }

        public string UserTableName { get; set; }

        public string RoleTableName { get; set; }

    }
}
