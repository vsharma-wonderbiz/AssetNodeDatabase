using System.Threading.Tasks;
using AssetNode.Interface;
using AssetNode.Models.Dtos;
using AssetNode.Models.Entities;
using AssetNode.Services;
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

        [HttpPost]
        public async Task<IActionResult> AddSignal([FromBody] AddSignalDto dto)
        {
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


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSignal(int id, UpdateSignalDto dto)
        {
            try
            {
                var updated = await _signal.UpdateSignalAsync(id, dto);

                if (updated == null)
                    return NotFound(new { message = "Signal not found." });

                // Map entity → DTO before returning (good practice)
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
            catch (InvalidOperationException ex)
            {
                // Specific error: duplicate signal
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // General error
                return StatusCode(500, new { message = "Unexpected error occurred.", detail = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSignal(int id)
        {
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
