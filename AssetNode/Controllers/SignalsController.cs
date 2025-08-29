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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSignal(int id, AddSignalDto dto)
        {
            var updated = await _signal.UpdateSignalAsync(id, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated); // Entity directly return ho rahi hai
        }


    }
}
