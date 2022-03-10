using System;
using System.IO;
using System.Threading.Tasks;
using ImageApi.Azure;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using NUnit.Framework;


namespace TestImageApi.Tests
{
    internal class TestCvManagerImpl
    {

        #region private attributes
        private CvManager cvManager;
        private ImageAnalysis results;
        private string endPoint = Environment.GetEnvironmentVariable("AZURE_COGNITIVE_ENDPOINT") 
                                  ?? throw new Exception("No Azure Cognitive Service Endpoint set in environment variables (AZURE_COGNITIVE_ENDPOINT)");
        private string key = Environment.GetEnvironmentVariable("AZURE_COGNITIVE_KEY") 
                             ?? throw new Exception("No Azure Cognitive Service Pass set in environment variables (AZURE_COGNITIVE_KEY)");
        #endregion private attributes

        /// <summary>
        /// This test method initializes the context before each test method run.
        /// </summary>
        [SetUp]
        public void Init()
        {
            cvManager = new(endPoint, key);
        }

        [Test]
        public async Task CreateClient_Success()
        {
            Assert.IsNotNull(await cvManager.CreateClient());
        }

        [Test]
        public async Task AnalyzeImage_Success()
        {
            var client = await cvManager.CreateClient();

            results = await cvManager.AnalyzeImage(client, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTMXW6Dmka_PWSaR862RMAEAVMZCSqkpJp-CA&usqp=CAU");

            Assert.IsNotNull(results);
        }
    }
}
