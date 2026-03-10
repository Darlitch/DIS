using Contract.Api;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HashController(HashCrackService hashCrackService) : ControllerBase
{
    [HttpPost("crack")]
    public ActionResult<CrackResponseDto> CrackTask([FromBody] HashCrackDto dto)
    {
        var requestId = hashCrackService.StartCrack(dto);
        if (requestId == null)
        {
            return StatusCode(429, "Queue is full");
        } 
        return Ok(new CrackResponseDto(requestId.Value));
    }

    [HttpGet("status")]
    public ActionResult<CrackStatusDto> GetStatus([FromQuery] Guid requestId)
    {
        var dto = hashCrackService.GetRequestStatus(requestId);
        return Ok(dto);
    }
}