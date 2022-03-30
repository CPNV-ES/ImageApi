using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace ImageApi.Azure
{
    public class CvManager : ICvManager
    {
        private string endpoint;
        private string key;

        public CvManager(string endpoint, string key)
        {
            this.endpoint = endpoint;
            this.key = key;
        }

        /// <summary>
        /// Create the client used to get all computer vision functionality
        /// </summary>
        /// <param name="endpoint">Url of the ressource</param>
        /// <param name="key">Subscription key</param>
        /// <returns>Computer Vision client</returns>
        public async Task<ComputerVisionClient> CreateClient()
        {
            ComputerVisionClient client =
            new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };


            return client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public async Task<ImageAnalysis> AnalyzeImage(ComputerVisionClient client, string imageUrl, float minConfidence, int maxLabels)
        {
            // Creating a list that defines the features to be extracted from the image. 

            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            var analysis = await client.AnalyzeImageAsync(imageUrl, features);
            analysis.Tags = analysis.Tags.Where(t => t.Confidence > minConfidence).OrderByDescending(t => t.Confidence).Take(maxLabels).ToList();
            analysis.Objects = analysis.Objects.Where(o => o.Confidence > minConfidence).ToList();


            return analysis;
        }
    }
}
