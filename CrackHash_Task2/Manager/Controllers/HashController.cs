using Contract.Api;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HashController(HashCrackService hashCrackService) : ControllerBase
{
    [Consumes("application/json")]
    [HttpPost("crack")]
    public async Task<ActionResult<CrackResponseDto>> CrackTask([FromBody] HashCrackDto dto)
    {
        Guid? requestId;
        try
        {
            requestId = await hashCrackService.StartCrack(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(503, ex.ToString());
        }
        if (requestId == null)
        {
            return StatusCode(429, "Queue is full. Please try again later.");
        } 
        return Ok(new CrackResponseDto(requestId.Value));
    }

    [Consumes("application/json")]
    [HttpGet("requestStatus")]
    public async Task<ActionResult<CrackStatusDto>> GetStatus([FromQuery] Guid requestId)
    {
        var dto = await hashCrackService.GetRequestStatus(requestId);
        return Ok(dto);
    }
}