using Microsoft.AspNetCore.Mvc;

namespace AssetNode.Controllers
{
    public class DebugContoller : Controller
    {
        [HttpGet("debug-claims")]
        public IActionResult DebugClaims()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }
    }
}
