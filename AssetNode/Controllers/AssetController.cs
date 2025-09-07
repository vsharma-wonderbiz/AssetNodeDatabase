using AssetNode.Data;
using AssetNode.Interface;
using AssetNode.Models.Dtos;
using AssetNode.Models.Entities;
using AssetNode.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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

        [AllowAnonymous]
        [HttpPost("upload")]
        //public async Task<IActionResult> ImportFromFile(IFormFile file)
        //{
        //    var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        //    Console.WriteLine($"upload file request with token: {authHeader}");

        //    //try
        //    //{
        //    //    if (file == null || file.Length == 0)
        //    //        return BadRequest(new { error = "No file uploaded." });

        //    //    List<FileAssetDto> assets = new List<FileAssetDto>();
        //    //    using (var reader = new StreamReader(file.OpenReadStream()))
        //    //    {
        //    //        string content = await reader.ReadToEndAsync();
        //    //        assets = JsonConvert.DeserializeObject<List<FileAssetDto>>(content);
        //    //    }

        //    //    await _sqlinterface.ImportFromFile(assets);
        //    //    return Ok(new { message = "File imported successfully." });
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    return BadRequest(new { error = ex.Message });
        //    //}
        //    if(file==null || file.Length == 0)
        //    {
        //        return BadRequest("File is Empty or not uploaded");
        //    }

        //    string content;
        //    using(var Reader=new StreamReader(file.OpenReadStream()))
        //    {
        //        content = await Reader.ReadToEndAsync();
        //    }

        //    try
        //    {
        //        JArray.Parse(content);
        //    }
        //    catch(JsonReaderException ex)
        //    {
        //        return BadRequest($"Invalid format: {ex.Message}");
        //    }
        //    List<FileAssetDto> assets;
        //    try
        //    {
        //        assets = JsonConvert.DeserializeObject<List<FileAssetDto>>(content);
        //    }
        //    catch(Exception ex)
        //    {
        //        return BadRequest($"faliure {ex.Message}");
        //    }


        //    if (assets == null || !assets.Any())
        //        return BadRequest("No valid assets found in the file.");


        //    foreach (var asset in assets)
        //    {
        //        if (asset == null)
        //            return BadRequest("One of the asset records is null.");

        //        if (string.IsNullOrWhiteSpace(asset.Name))
        //            return BadRequest($"Asset with Id {asset.TempId} has an empty Name.");

        //        if (asset.TempId <= 0)
        //            return BadRequest("Asset Id must be greater than 0.");
        //    }

        //    // ✅ Business validations will be in Service Layer
        //     await _sqlinterface.ImportFromFile(assets);

        //    return Ok(new { message = "Import successful" });

        //}
        public async Task<IActionResult> ImportFromFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    errors = new[] { new { field = "file", message = "File is empty or not uploaded" } }
                });
            }

            string content;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                content = await reader.ReadToEndAsync();
            }

            JArray jsonArray;
            try
            {
                jsonArray = JArray.Parse(content);
            }
            catch (JsonReaderException)
            {
                return BadRequest(new
                {
                    errors = new[] { new { field = "file", message = "Invalid JSON format" } }
                });
            }

            var allowedKeys = new HashSet<string> { "Id", "Name", "ParentId" };
            var validationErrors = new List<object>();

            for (int i = 0; i < jsonArray.Count; i++)
            {
                var obj = (JObject)jsonArray[i];

                foreach (var property in obj.Properties())
                {
                    string key = property.Name;
                    if (!allowedKeys.Contains(key))
                    {
                        validationErrors.Add(new
                        {
                            index = i,
                            field = key,
                            message = $"At Record :{i+1} -> Invalid key name '{key}'"
                        });
                    }
                }

               
                if (obj.TryGetValue("Id", out JToken idToken))
                {
                    if (!int.TryParse(idToken.ToString(), out int id) || id <= 0)
                    {
                        validationErrors.Add(new
                        {
                            index = i,
                            field = "Id",
                            message = "Id must be a positive integer"
                        });
                    }
                }
                else
                {
                    validationErrors.Add(new { index = i, field = "Id", message = "Missing required key 'Id'" });
                }

                
                if (obj.TryGetValue("Name", out JToken nameToken))
                {
                    string name = nameToken.ToString();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        validationErrors.Add(new
                        {
                            index = i,
                            field = "Name",
                            message = $"At Reacord :{i+1} -> Name Cannot be Empty"
                        });
                    }
                    else if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9]+$"))
                    {
                        validationErrors.Add(new
                        {
                            index = i,
                            field = "Name",
                            message = $"At Record:{i+1} - >Name can only contain letters and numbers (no spaces or special characters)"
                        });
                    }                                   
                }
                else
                {
                    validationErrors.Add(new { index = i, field = "Name", message = "Missing required key 'Name'" });
                }

               
                if (obj.TryGetValue("ParentId", out JToken parentIdToken))
                {
                    if (!parentIdToken.Type.Equals(JTokenType.Null))
                    {
                        if (!int.TryParse(parentIdToken.ToString(), out _))
                        {
                            validationErrors.Add(new
                            {
                                index = i,
                                field = "ParentId",
                                message = $"At Record :{i} -> ParentId must be an integer or null"
                            });
                        }
                    }
                }   
                else
                {
                    validationErrors.Add(new { index = i, field = "ParentId", message = "Missing required key 'ParentId'" });
                }
            }

            
            if (validationErrors.Any())
            {
                return BadRequest(new { errors = validationErrors });
            }

            var assets = jsonArray.ToObject<List<FileAssetDto>>();
            await _sqlinterface.ImportFromFile(assets);

            return Ok(new { message = "File imported successfully" });
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


        ////[HttpGet]
        ////public async Task<ActionResult<IEnumerable<HitoryLog>>> GetAllHistoryLogs()
        //{
        //    try
        //    {
        //        var logs = await _context.HitoryLogs
        //            .OrderByDescending(h => h.ChangedAt)
        //            .ToListAsync();

        //        return Ok(logs);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error: {ex.Message}");
        //    }
        //}

    }
}
