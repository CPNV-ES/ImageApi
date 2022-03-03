using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using ImageApi.Azure;
using NUnit.Framework;


namespace TestImageApi.Tests
{
    /// <summary>
    /// This test class is designed to confirm the AwsBucketManager class's behavior
    /// </summary>
    public class TestAzureBlobManagerImpl
    {

        #region private attributes
        private AzureBlobManager bucketManager;

        private string blobName;
        private string containerName;
        private string pathToTestFolder;
        
        
        #endregion private attributes

        /// <summary>
        /// This test method initializes the context before each test method run.
        /// </summary>
        [SetUp]
        public void Init()
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTIONSTRING") ?? throw new Exception("No Azure Storage Connection String");
            
            this.pathToTestFolder = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.ToString(), "testData");
            this.containerName = "testbucket";
            this.blobName= "emiratesa380.jpg";
            this.bucketManager = new AzureBlobManager(connectionString);

            Cleanup();
        }

        /// <summary>
        /// This test method checks the method in charge of creating a new object
        /// We try to create a new bucket
        /// </summary>
        [Test]
        public async Task CreateObject_CreateNewBucket_Success()
        {
            //given
            Assert.IsFalse(await this.bucketManager.ObjectExists(containerName));

            //when
            await CreateObjectWhenAvailable(containerName);

            //then
            Assert.IsTrue(await this.bucketManager.ObjectExists(containerName));
        }

        /// <summary>
        /// This test method checks the method in charge of creating a new data object
        /// Note : the bucket exists
        /// </summary>
        [Test]
        public async Task CreateObject_CreateObjectWithExistingBucket_Success()
        {
            //given
            string fileName = this.blobName;
            string objectUrl = this.containerName + "/" + this.blobName;

            await CreateObjectWhenAvailable(this.containerName);
            
            Assert.IsTrue(await this.bucketManager.ObjectExists(this.containerName));
            Assert.IsFalse(await this.bucketManager.ObjectExists(objectUrl));

            //when
            await this.bucketManager.CreateObject(objectUrl, Path.Combine(this.pathToTestFolder, fileName));

            //then
            Assert.IsTrue(await this.bucketManager.ObjectExists(objectUrl));
        }

        /// <summary>
        /// This test method checks the method in charge of creating a new data object
        /// Note : the bucket doesn't exist
        /// </summary>
        [Test]
        public async Task CreateObject_CreateObjectBucketNotExist_Success()
        {
            //given
            string fileName = this.blobName;
            string objectUrl = this.containerName + "/" + this.blobName;
            Assert.IsFalse(await this.bucketManager.ObjectExists(this.containerName));
            Assert.IsFalse(await this.bucketManager.ObjectExists(objectUrl));

            //when
            await CreateObjectWhenAvailable(objectUrl, Path.Combine(this.pathToTestFolder, fileName));

            //then
            Assert.IsTrue(await this.bucketManager.ObjectExists(objectUrl));
        }

        /// <summary>
        /// This test method checks the method in charge of uploading item in an existing bucket
        /// </summary>
        [Test]
        public async Task DownloadObject_NominalCase_Success()
        {
            //given
            string objectUrl = containerName + "//" + this.blobName;
            string destinationFullPath = this.pathToTestFolder + "//" + this.blobName;
            await this.bucketManager.CreateObject(objectUrl, this.pathToTestFolder + "//" + this.blobName);

            Assert.IsTrue(await this.bucketManager.ObjectExists(containerName));

            //when
            await this.bucketManager.DownloadObject(objectUrl, destinationFullPath);

            //then
            Assert.IsTrue(File.Exists(destinationFullPath));
        }

        /// <summary>
        /// This test method checks the method in charge of testing the existence of an object
        /// </summary>
        [Test]
        public async Task IsObjectExists_NominalCase_Success()
        {
            //given
            Task t = this.bucketManager.CreateObject(this.containerName);
            await t;
            bool actualResult;

            //when
            actualResult = await this.bucketManager.ObjectExists(containerName);

            //then
            Assert.IsTrue(actualResult);
        }

        /// <summary>
        /// This test method checks the method in charge of testing the existence of an object
        /// When the object doesn't exist (object is the bucket)
        /// </summary>
        [Test]
        public async Task IsObjectExists_ObjectNotExistBucket_Success()
        {
            //given
            string notExistingBucket = "notExistingBucket" ;
            bool actualResult;

            //when
            actualResult = await this.bucketManager.ObjectExists(notExistingBucket);

            //then
            Assert.IsFalse(actualResult);
        }

        /// <summary>
        /// This test method checks the method in charge of testing the existence of an object
        /// When the object doesn't exist (object is the file in an existing bucket)
        /// </summary>
        [Test]
        public async Task IsObjectExists_ObjectNotExistFile_Success()
        {
            //given
            await this.bucketManager.CreateObject(this.containerName);
            string notExistingFile = containerName + "//" + "notExistingFile.jpg";
            Assert.IsTrue(await this.bucketManager.ObjectExists(containerName));
            bool actualResult;

            //when
            actualResult = await this.bucketManager.ObjectExists(notExistingFile);

            //then
            Assert.IsFalse(actualResult);
        }

        /// <summary>
        /// This test method checks the method in charge of removing an existing object
        /// Case : empty bucket
        /// </summary>
        [Test]
        public async Task RemoveObject_EmptyBucket_Success()
        {
            //given
            await this.bucketManager.CreateObject(this.containerName);
            Assert.IsTrue(await this.bucketManager.ObjectExists(containerName));

            //when
            await this.bucketManager.RemoveObject(this.containerName);

            //then
            Assert.IsFalse(await this.bucketManager.ObjectExists(containerName));
        }

        /// <summary>
        /// This test method checks the method in charge of removing an existing object
        /// Case : bucket with content
        /// </summary>
        [Test]
        public async Task RemoveObject_NotEmptyBucket_Success()
        {
            //given
            string fileName = this.blobName;
            string objectUrl = this.containerName + "/" + this.blobName;
            await this.bucketManager.CreateObject(this.containerName);
            await this.bucketManager.CreateObject(objectUrl, this.pathToTestFolder + "//" + fileName);

            Assert.IsTrue(await this.bucketManager.ObjectExists(containerName));
            Assert.IsTrue(await this.bucketManager.ObjectExists(objectUrl));

            //when
            await this.bucketManager.RemoveObject(this.containerName);

            //then
            Assert.IsFalse(await this.bucketManager.ObjectExists(containerName));
        }

        /// <summary>
        /// This test method cleans up the context after each test method run.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            string destinationFullPath = Path.Combine(this.pathToTestFolder, "out", this.blobName);

            if (File.Exists(destinationFullPath))
            {
                File.Delete(destinationFullPath);
            }

            if (this.bucketManager.ObjectExists(containerName).Result)
            {
                this.bucketManager.RemoveObject(this.containerName).Wait();
            }
        }

        /// <summary>
        /// Because we delete the container after each test, we need to make sure it's finished being deleted before we create it again.
        /// This method creates a new object and retries after 3 seconds if the container is not yet deleted.
        /// It stills throws for every other type of exceptions.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="filePath"></param>
        private async Task CreateObjectWhenAvailable(string containerName, string filePath = "")
        {
            do {
                try {
                    // If there's no filepath, we create only a container
                    if (filePath == "")
                        await this.bucketManager.CreateObject(containerName);
                    else
                        await this.bucketManager.CreateObject(containerName, filePath);
                    
                    break;
                }
                catch(RequestFailedException ex) {
                    // 409: The speficied container is being deleted. Try operation later.
                    if(ex.Status == 409) 
                        Thread.Sleep(3000); // If it's that specific error, try again after 3 seconds   
                    else 
                        throw;  // Else just return the exception
                    
                }
            } while(true); // TODO: Set up a timeout or max retries
        }
    }
}