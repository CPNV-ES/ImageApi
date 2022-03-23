using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace ImageApi.Azure
{
    public interface ICvManager
    {
        Task<ComputerVisionClient> CreateClient();
        Task<ImageAnalysis> AnalyzeImage(ComputerVisionClient client, string imageUrl, float minConfidence, int maxLabels);
    }
}
