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
        #endregion private attributes

        /// <summary>
        /// This test method initializes the context before each test method run.
        /// </summary>
        [SetUp]
        public void Init()
        {
            cvManager = new("https://ria2-cognitiveservice.cognitiveservices.azure.com/", "43a08f2d34f34f60a906c85f387003eb");
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
