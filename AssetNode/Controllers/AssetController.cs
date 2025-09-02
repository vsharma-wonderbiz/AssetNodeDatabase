using Microsoft.AspNetCore.Mvc;
using AssetNode.Models.Dtos;
using AssetNode.Data;
using AssetNode.Services;
using AssetNode.Interface;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;

namespace AssetNode.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetController : ControllerBase
    {

        private readonly IJsonAssetInterface _jsonAssetInterface;
        private readonly IAssetStorage _storageservice;
        private readonly IWebHostEnvironment _env;
        private readonly ISqlInterface _sqlinterface;

        public AssetController(IJsonAssetInterface jsonAssetInterface,IAssetStorage storage, IWebHostEnvironment env, ISqlInterface sqlinterface)
        {
            _jsonAssetInterface = jsonAssetInterface;
            _storageservice = storage;
            _env = env;
            _sqlinterface = sqlinterface;

        }
        //post asset
        [HttpPost]

        public async Task<IActionResult> AddAsset([FromBody] SqladdAsset assetDto)
        {
           
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized(new { message = "Authorization header missing" });
            }

            Console.WriteLine($"Authorization Header: {authHeader}"); // Console logging
                                                                      // ya phir
                                                                      // _logger.LogInformation("Authorization Header: {authHeader}", authHeader);

            try
            {
                var created = await _sqlinterface.AddAsset(assetDto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpGet("heirarchy")]
        public  async Task<IActionResult> GetHierarchy()
        {
            var root = await _sqlinterface.GetJsonHierarchy();
            return Ok(root); // Returns full hierarchy as JSON
        }



        [HttpGet("Statistics")]
        public IActionResult GetCount()
        {
          
            int totalCount = _sqlinterface.DisplayCount().GetAwaiter().GetResult();
            int maxDepth = _sqlinterface.MaxDepth().GetAwaiter().GetResult();

            return Ok(new
            {
                TotalNodes = totalCount,
                MaxDepth = maxDepth
            });

        }

        [HttpPost("upload")]
        
        public async Task<IActionResult> ImportFromFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded." });

                List<FileAssetDto> assets = new List<FileAssetDto>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    string content = await reader.ReadToEndAsync();
                    assets = JsonConvert.DeserializeObject<List<FileAssetDto>>(content);
                }

                await _sqlinterface.ImportFromFile(assets);
                return Ok(new { message = "File imported successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }




        [HttpGet("download")]
        public IActionResult DownloadFile()
        {
            string filepath = Path.Combine(_env.ContentRootPath, "Data/JsonH.json");
            byte[] filbytes = System.IO.File.ReadAllBytes(filepath);

            string contenttype = "application/json";

            return File(filbytes, contenttype,"assest");
        }


        [HttpDelete]
        [Route("{Id}")]
        public IActionResult DeleteNode(int Id)
        {
            try
            {
                _sqlinterface.DeleteNode(Id);
                return Ok(new { message = "Asset deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
