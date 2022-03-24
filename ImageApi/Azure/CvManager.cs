using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        public async Task<ImageAnalysis> AnalyzeImage(ComputerVisionClient client, string imageUrl)
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
                                                                                   
            var sql = ImageAnalysisToSql(analysis, imageUrl); 
            
            return analysis;
        }

        public static string ImageAnalysisToSql(ImageAnalysis analysis, string imageUrl)
        {
            var sql = new StringBuilder();
            sql.AppendLine($"insert into `image` values (0, '{imageUrl})', '<name>', '<hash>');");
            sql.AppendLine($"insert into `analyse` values (0, LAST_INSERT_ID(), '<ip>', '2021-01-01', '2021-01-01');");

            sql.AppendLine("declare @analyse_id as int = LAST_INSERT_ID();");
            foreach(var tag in analysis.Tags)
            {
                sql.AppendLine($"insert into `object` values (0, @analyse_id, 'tag')");
                sql.AppendLine("insert into `attribute` values ");
                sql.AppendLine("(0, LAST_INSERT_ID(), 'name', '" + tag.Name + "'),");
                sql.AppendLine("(0, LAST_INSERT_ID(), 'confidence', '" + tag.Confidence+ "'),");
            }
            
            return sql.ToString();
        }
    }
}
