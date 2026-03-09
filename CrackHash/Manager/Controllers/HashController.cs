using Contract.Api;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HashController(HashCrackService hashCrackService) : ControllerBase
{
    [HttpPost("crack")]
    public async Task<ActionResult<CrackResponseDto>> CrackTask([FromBody] HashCrackDto dto)
    {
        var requestId = await hashCrackService.StartCrackAsync(dto);
        return Ok(new CrackResponseDto(requestId));
    }

    [HttpGet("status")]
    public ActionResult<CrackStatusDto> GetStatus([FromQuery] Guid requestId)
    {
        var dto = hashCrackService.GetRequestStatus(requestId);
        return Ok(dto);
    }
}