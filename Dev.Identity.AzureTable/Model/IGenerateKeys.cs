namespace Dev.Identity.AzureTable.Model
{
    public interface IGenerateKeys
    {
        void GenerateKeys(IKeyHelper keyHelper);

        string PeekRowKey(IKeyHelper keyHelper);

        double KeyVersion { get; set; }
    }
}
