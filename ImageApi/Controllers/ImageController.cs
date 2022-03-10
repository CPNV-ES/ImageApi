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
        public ImageController(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
         
        }
        

        // POST api/<ImageController>
        [HttpPost]
        public async Task<ImageAnalysis> Post(IFormFile file)
        {
            //  A REFACTOR 
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
           
            await _blobManager.CreateObject("imganalysis/" + file.FileName, filePath);

            var uri  = _blobManager.GetServiceSasUriForBlob(file.FileName, "imganalysis");
            var client = await _cvManager.CreateClient();

            // Traité en interne

            ImageAnalysis imageAnalized = await _cvManager.AnalyzeImage(client, uri);
            await _blobManager.RemoveObject("imganalysis/" + file.FileName);
            return imageAnalized;
        }
    }
}
