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
            _blobManager = new AzureBlobManager(Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTIONSTRING"));
            _cvManager = new CvManager("https://ria2-cognitiveservice.cognitiveservices.azure.com/", "***REMOVED***");
        }
        // GET: api/<ImageController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ImageController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ImageController>
        [HttpPost]
        public async Task<ImageAnalysis> Post(IFormFile file)
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
           
            await this._blobManager.CreateObject("imganalysis/" + file.FileName, filePath);
            
            var client = await this._cvManager.CreateClient();
            
            ImageAnalysis imageAnalized = await this._cvManager.AnalyzeImage(client, "https://storageaccountria2.blob.core.windows.net/imganalysis/"+file.FileName);
            await _blobManager.RemoveObject("imganalysis/" + file.FileName);
            return imageAnalized;
        }

        // PUT api/<ImageController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ImageController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
