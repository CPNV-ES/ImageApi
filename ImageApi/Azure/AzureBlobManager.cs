using Azure.Storage.Blobs;

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

        /// <summary>
        /// Remove object from 
        /// </summary>
        /// <param name="objectUrl"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task RemoveObject(string objectUrl)
        {
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            await blobServiceClient.DeleteBlobContainerAsync(connectionString);
        }
    }
}
