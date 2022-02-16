namespace ImageApi.Azure
{
    public interface IBlobManager
    {
        Task CreateObject(string objectUrl, string filePath = "");
        Task<Boolean> ObjectExists(string objectUrl);
        Task RemoveObject(string objectUrl);
        Task DownloadObject(string objectUrl, string destinationUri);
    }
}
