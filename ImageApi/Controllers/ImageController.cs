using ImageApi.Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ImageApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private IWebHostEnvironment _hostingEnvironment;
        private AzureBlobManager _blobManager;
        private CvManager _cvManager;
        private string cognitiveEndPoint = Environment.GetEnvironmentVariable("AZURE_COGNITIVE_ENDPOINT") 
                                           ?? throw new Exception("No Azure Cognitive Service Endpoint set in environment variables (AZURE_COGNITIVE_ENDPOINT)");
        private string cognitiveKey = Environment.GetEnvironmentVariable("AZURE_COGNITIVE_KEY") 
                                      ?? throw new Exception("No Azure Cognitive Service Pass set in environment variables (AZURE_COGNITIVE_KEY)");
        private string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING") 
                                          ?? throw new Exception("No Azure Storage Connection String set in environment variables (AZURE_STORAGE_CONNECTION_STRING)");
        
        public ImageController(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
         
        }
        

        // POST api/<ImageController>
        [HttpPost]
        public async Task<ImageAnalysis> AnalysisImage(IFormFile file)
        {
            string filePath = await SaveImageToDisk(file);

            string uri = await SaveImageToAzure("imganalysis",file.FileName, filePath);

            ImageAnalysis imageAnalized = await sendFileToAnalyze(uri);
            await RemoveImageFromAzure("imganalysis", file.FileName);
            return imageAnalized;
        }

        private async Task<string> SaveImageToDisk(IFormFile file)
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads");
            string filePath = "";
            if (file.Length > 0)
            {
                filePath = Path.Combine(uploads, file.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            return filePath;
        }
        private async Task<string> SaveImageToAzure(string container, string fileName, string filePath)
        {
            string objectUri = container + "/" + fileName;
            await _blobManager.CreateObject(objectUri, filePath);

            var uri = _blobManager.GetServiceSasUriForBlob(fileName, container);
            return uri;
        }
        private async Task<ImageAnalysis> sendFileToAnalyze(string uri)
        {
            var client = await _cvManager.CreateClient();

            ImageAnalysis imageAnalized = await _cvManager.AnalyzeImage(client, uri);
            //
            return imageAnalized;
        }

        private async Task<bool> RemoveImageFromAzure(string container, string fileName)
        {
            try
            {
                await _blobManager.RemoveObject(container + "/" + fileName);
                return true;
            } catch {
                throw new Exception("A problem occured during the suppression process.");
            }
            
        }
    }
}
