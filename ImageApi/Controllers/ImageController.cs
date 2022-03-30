using System.Security.Cryptography;
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
            _blobManager = new(connectionString);
            _cvManager = new(cognitiveEndPoint, cognitiveKey);
         
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">The in</param>
        /// <param name="minConfidence"></param>
        /// <param name="maxLabels"></param>
        /// <returns></returns>
        // POST api/<ImageController>
        [HttpPost]
        public async Task<IActionResult> AnalysisImage(IFormFile file, [FromForm] float minConfidence, [FromForm] int maxLabels)
        {
            minConfidence = minConfidence == 0 ? 0.8f : minConfidence;
            maxLabels = maxLabels == 0 ? 10 : maxLabels;

            if (minConfidence < 0 || minConfidence > 1 || maxLabels < 0 || !CheckFileExtension(file))
                return BadRequest();
            

            string filePath = await SaveImageToDisk(file);

            string uri = await SaveImageToAzure("imganalysis",file.FileName, filePath);

            ImageAnalysis imageAnalized = await sendFileToAnalyze(uri,minConfidence,maxLabels);

            using var sha = SHA256.Create();
            using var streamReader = new StreamReader(file.OpenReadStream());
            var hash = BitConverter.ToString(sha.ComputeHash(streamReader.BaseStream)).Replace("-", "").ToLower();
            var analysisData = new ImageAnalysisHelpers.AnalysisQueryData
            {
                ImageHash = hash,
                ImageName = file.FileName,
                ImageUrl = uri,
                ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            };
            var sql = ImageAnalysisHelpers.ImageAnalysisToSql(imageAnalized, analysisData);
            
            await RemoveImageFromAzure("imganalysis", file.FileName);
            return Ok(imageAnalized);
        }

        private bool CheckFileExtension(IFormFile file)
        {
            var supportedTypes = new[] { "jpg", "png", "jpeg", "bmp", "svg"};
            var fileExt = Path.GetExtension(file.FileName).Substring(1);
            if (!supportedTypes.Contains(fileExt))
            {
                return false;
            }
            else
            {
                return true;
            }  
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
        
        private async Task<ImageAnalysis> sendFileToAnalyze(string uri, float minConfidence, int maxLabel)
        {
            var client = await _cvManager.CreateClient();

            ImageAnalysis imageAnalized = await _cvManager.AnalyzeImage(client, uri, minConfidence, maxLabel);
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
