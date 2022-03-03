using System;
using System.IO;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ImageApi.Azure
{
    public class AzureBlobManager : IBlobManager
    {

        private BlobServiceClient blobServiceClient;
        private BlobContainerClient blobClient;
        private string storageConnectionString; 
        
        public AzureBlobManager(string connectionString)
        {
            storageConnectionString = connectionString;
            blobServiceClient = new BlobServiceClient(storageConnectionString);
        }

        public async Task CreateObject(string objectUrl)
        {
            await blobServiceClient.CreateBlobContainerAsync(objectUrl);
        }

        public async Task CreateObject(string objectUrl, string filePath)
        {
            var (containerName, fileName) = parseObjectUrl(objectUrl);
            
            var blobContainerClient = new BlobContainerClient(storageConnectionString, containerName);

            // Create the container if it doesn't already exist.
            if (!await blobContainerClient.ExistsAsync()) {
                await CreateObject(containerName);
            }
            
            var blob = blobContainerClient.GetBlobClient(fileName);
            
            await using var fs = File.OpenRead(filePath);
            await blob.UploadAsync(filePath);
        }

        public Task DownloadObject(string objectUrl, string destinationUri)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ObjectExists(string objectUrl)
        {
            var (containerName, fileName) = parseObjectUrl(objectUrl);
            
            var blobContainerClient = new BlobContainerClient(storageConnectionString, containerName);
            var blob = blobContainerClient.GetBlobClient(fileName);

            // If there's no file name, it's a container
            if (fileName == "")
                return blobContainerClient.ExistsAsync().Result;
            else
                return await blob.ExistsAsync();
        }
        
        public async Task RemoveObject(string objectUrl)
        {
            var (containerName, fileName) = parseObjectUrl(objectUrl);
            
            var blobContainerClient = new BlobContainerClient(storageConnectionString, containerName);
            var blob = blobContainerClient.GetBlobClient(fileName);

            // If there's no file name, it's a container
            if (fileName == "") 
                await blobContainerClient.DeleteAsync();    // Delete the container
            else
                await blob.DeleteAsync();   // Delete the blob
        }
        
        /// <summary>
        /// Parses the object url into a tuple of container name and file name
        /// </summary>
        /// <param name="objectUrl"></param>
        /// <returns>A tuple containing the container name as its first element and an eventual file name as its second element</returns>
        private (string, string) parseObjectUrl(string objectUrl) 
        {
            var fileName = objectUrl.Split("/").Length > 1 ? string.Join('/', objectUrl.Split("/")[1..]) : "";
            var containerName = objectUrl.Split("/")[0];
            return (containerName, fileName);
        }
    }
}
