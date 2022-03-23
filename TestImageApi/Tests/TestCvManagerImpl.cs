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

            results = await cvManager.AnalyzeImage(client, "https://images.squarespace-cdn.com/content/v1/5005bedd84ae929b3720c30f/1544403472316-BLKSLUI25Q69QIO8DWM8/jeux+en+famille+dans+les+party+de+No%C3%ABl-+temps+de+F%C3%AAtes-jouer-rire-id%C3%A9es+de+jeux-famille-enfant-Je+suis+une+maman?format=1000w", 0.8f, 3);

            Assert.IsNotNull(results);
        }
    }
}
