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
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using System.Runtime.Intrinsics.X86;

namespace AssetNode.Controllers
{
    [ApiController]
    [Authorize]
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
        [Authorize(Roles ="Admin")]
        [HttpPost("Add")]
        public async Task<IActionResult> AddAsset([FromBody] SqladdAsset assetDto)
        {
           
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized(new { message = "Authorization header missing" });
            }

            Console.WriteLine($"Authorization Header: {authHeader}"); 

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


        [AllowAnonymous]
        [HttpGet("heirarchy")]
        public  async Task<IActionResult> GetHierarchy()
        {
            var root = await _sqlinterface.GetJsonHierarchy();
            return Ok(root); // Returns full hierarchy as JSON
        }


        [AllowAnonymous]
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

            [Authorize(Roles = "Admin")]
            [HttpPost("upload")]
            public async Task<IActionResult> ImportFromFile(IFormFile file)
            {
                var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine($"upload file request with token: {authHeader}");
           
                //try
                //{
                //    if (file == null || file.Length == 0)
                //        return BadRequest(new { error = "No file uploaded." });

                //    List<FileAssetDto> assets = new List<FileAssetDto>();
                //    using (var reader = new StreamReader(file.OpenReadStream()))
                //    {
                //        string content = await reader.ReadToEndAsync();
                //        assets = JsonConvert.DeserializeObject<List<FileAssetDto>>(content);
                //    }

                //    await _sqlinterface.ImportFromFile(assets);
                //    return Ok(new { message = "File imported successfully." });
                //}
                //catch (Exception ex)
                //{
                //    return BadRequest(new { error = ex.Message });
                //}
                if(file==null || file.Length == 0)
                {
                    return BadRequest("File is Empty or not uploaded");
                }

                string content;
                using(var Reader=new StreamReader(file.OpenReadStream()))
                {
                    content = await Reader.ReadToEndAsync();
                }

                try
                {
                    JArray.Parse(content);
                }
                catch(JsonReaderException ex)
                {
                    return BadRequest($"Invalid format: {ex.Message}");
                }
                List<FileAssetDto> assets;
                try
                {
                    assets = JsonConvert.DeserializeObject<List<FileAssetDto>>(content);
                }
                catch(Exception ex)
                {
                    return BadRequest($"faliure {ex.Message}");
                }

            
                if (assets == null || !assets.Any())
                    return BadRequest("No valid assets found in the file.");

            
                foreach (var asset in assets)
                {
                    if (asset == null)
                        return BadRequest("One of the asset records is null.");

                    if (string.IsNullOrWhiteSpace(asset.Name))
                        return BadRequest($"Asset with Id {asset.TempId} has an empty Name.");

                    if (asset.TempId <= 0)
                        return BadRequest("Asset Id must be greater than 0.");
                }

                // ✅ Business validations will be in Service Layer
                 await _sqlinterface.ImportFromFile(assets);

                return Ok(new { message = "Import successful" });

            }



        [AllowAnonymous]
        [HttpGet("download")]
        public IActionResult DownloadFile()
        {
            string filepath = Path.Combine(_env.ContentRootPath, "Data/JsonH.json");
            byte[] filbytes = System.IO.File.ReadAllBytes(filepath);

            string contenttype = "application/json";

            return File(filbytes, contenttype,"assest");
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteNode(int id)
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"Delete node request with token: {authHeader}");

            try
            {
                _sqlinterface.DeleteNode(id);
                return Ok(new { message = "Asset deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
