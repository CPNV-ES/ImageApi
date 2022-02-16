namespace ImageApi.Azure
{
    public class AzureBlobManager : IBlobManager
    {

        public AzureBlobManager(string bucketUrl)
        {
            throw new NotImplementedException();
        }

        public Task CreateObject(string objectUrl, string filePath = "")
        {
            throw new NotImplementedException();
        }

        public Task DownloadObject(string objectUrl, string destinationUri)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ObjectExists(string objectUrl)
        {
            throw new NotImplementedException();
        }

        public Task RemoveObject(string objectUrl)
        {
            throw new NotImplementedException();
        }
    }
}
