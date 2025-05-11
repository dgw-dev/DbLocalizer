using Entities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DbLocalizer.Controllers
{
    [EnableCors("DbLocalizerCorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        //[HttpGet("GenerateKey")]
        //[ProducesResponseType<ReturnData>(StatusCodes.Status200OK)]
        //public IActionResult GenerateKey()
        //{
        //    string result = RandomKeyGenerator.GenerateHS256Key();
        //    return Ok(result);
        //}

        [HttpGet("GetToken")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public IActionResult GetToken(string appKey)
        {
            string result = SecureKeysManager.GenerateJwtToken(appKey);
            return Ok(result);
        }
    }
}
