using System.Threading.Tasks;
using AssetNode.Interface;
using AssetNode.Models.Dtos;
using AssetNode.Models.Entities;
using AssetNode.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AssetNode.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignalsController : ControllerBase
    {
        private readonly ISignalInterface _signal;

        public SignalsController(ISignalInterface signalInterface)
        {
            _signal = signalInterface;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSignlas()
        {
            try
            {
                var Allsignals = await _signal.GetSignals();
                return Ok(Allsignals);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }

        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddSignal([FromBody] AddSignalDto dto)
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"Add signal request with token: {authHeader}");
            try
            {
                var newsignal = await _signal.AddSignal(dto);
                return Ok(newsignal);
            }catch(Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateSignal(int id, UpdateSignalDto dto)
        //{
        //    try
        //    {
        //        var updated = await _signal.UpdateSignalAsync(id, dto);
        //        if (updated == null)
        //            return NotFound();

        //        return Ok(updated);
        //    }catch(Exception ex)
        //    {
        //        return BadRequest(new { ex.Message });
        //    }// Entity directly return ho rahi hai
        //}

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSignal(int id, UpdateSignalDto dto)
        {

            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"Edit signal request with token: {authHeader}");
            try
            {
                var updated = await _signal.UpdateSignalAsync(id, dto);

                var response = new
                {
                    updated.SignalId,
                    updated.SignalName,
                    updated.ValueType,
                    updated.Description,
                    updated.AssetID
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSignal(int id)
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"Delete signal request with token: {authHeader}");
            try
            {
                var reult = await _signal.DeleteSignal(id);
                if (reult == null)
                {
                    return BadRequest(reult);
                }
                else
                {
                    return Ok(reult);
                }
            }catch(Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }
        

    }
}
