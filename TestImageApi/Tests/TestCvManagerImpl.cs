﻿using System;
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
            cvManager = new(Environment.GetEnvironmentVariable("AZURE_COGNITIVE_ENDPOINT"), Environment.GetEnvironmentVariable("AZURE_COGNITIVE_KEY"));
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

            results = await cvManager.AnalyzeImage(client, "https://storageaccountria2.blob.core.windows.net/imganalysis/t%C3%A9l%C3%A9chargement.png?sp=r&st=2022-03-10T10:21:51Z&se=2022-03-10T18:21:51Z&spr=https&sv=2020-08-04&sr=b&sig=b1nYCt0J2TYKUVLNN4pXRPlNLoFcsJHdIAOj3oSRoW4%3D");

            Assert.IsNotNull(results);
        }
    }
}
