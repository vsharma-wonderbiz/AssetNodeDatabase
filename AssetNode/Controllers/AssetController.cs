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
            //json addd asset
            //var heirarchy = _storageservice.LoadHierarchy();
            //try
            //{
            //    if (heirarchy == null)
            //    {
            //        heirarchy = new List<AssetNodes>();
            //        _jsonAssetInterface.AddAsset(heirarchy, assetDto);
            //        _storageservice.SaveHierarchy(heirarchy);
            //    }
            //    else
            //    {
            //        _jsonAssetInterface.AddAsset(heirarchy, assetDto);
            //    }
            //}catch (Exception ex)
            //{
            //    return BadRequest(new { message = ex.Message });
            //}

            //    return Ok("Asset added successfully.");
            //Database
            try
            {
                var created = await _sqlinterface.AddAsset(assetDto);
                return Ok(created);
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpGet("heirarchy")]
        public  async Task<IActionResult> GetHierarchy()
        {
            //var root = _jsonAssetInterface.GetJsonHierarchy();

            //if (root == null || !root.Any())
            //{
            //    return Ok(new List<AssetNodes>());
            //}
            var root = await _sqlinterface.GetJsonHierarchy();
            return Ok(root); // Returns full hierarchy as JSON
        }



        [HttpGet("Statistics")]
        public IActionResult GetCount()
        {
            //int Totalcount = _jsonAssetInterface.DisplayCount();
            //int Maxdepth = _jsonAssetInterface.MaxDepth();
            //return Ok( new {
            //    TotalNodes=Totalcount,
            //     MaxDepth=Maxdepth
            //});
            int totalCount = _sqlinterface.DisplayCount().GetAwaiter().GetResult();
            int maxDepth = _sqlinterface.MaxDepth().GetAwaiter().GetResult();

            return Ok(new
            {
                TotalNodes = totalCount,
                MaxDepth = maxDepth
            });

        }

        [HttpPost("upload")]
        //public async Task<IActionResult> UploadFile(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("File is empty or missing.");

        //    using var reader = new StreamReader(file.OpenReadStream());
        //    string content = await reader.ReadToEndAsync();

        //    try
        //    {
        //        // Deserialize to List<AssetNodes>
        //        var uploadedNodes = JsonSerializer.Deserialize<List<AssetNodes>>(content, new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        });

        //        if (uploadedNodes == null)
        //        {
        //            return BadRequest("Invalid file content.");
        //        }

        //        // Check if uploaded file is flat
        //        bool isFlat = uploadedNodes.All(x => x.Children == null || x.Children.Count == 0);

        //        if (isFlat)
        //        {
        //            // Build hierarchy if flat file
        //            uploadedNodes = _jsonAssetInterface.ImportHeirarchyFromFile(uploadedNodes);
        //        }

        //        // Load existing hierarchy from storage
        //        var existingHierarchy = _storageservice.LoadHierarchy();
        //        if (existingHierarchy == null)
        //            existingHierarchy = new List<AssetNodes>();

        //        // Merge uploaded nodes into existing hierarchy
        //        var mergedHierarchy = _jsonAssetInterface.MergeHeirarchy(existingHierarchy, uploadedNodes);

        //        // Save merged hierarchy back to storage
        //        _storageservice.SaveHierarchy(mergedHierarchy);

        //        return Ok(mergedHierarchy);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
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
            //try
            //{
            //    var heirarchy = _storageservice.LoadHierarchy();
            //    _jsonAssetInterface.DeleteNode(heirarchy, Id);

            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(new { message = ex.Message });
            //}
            //return Ok($"Node with ID {Id} deleted successfully.");

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
