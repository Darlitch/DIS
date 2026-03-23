using Contract.Xml;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Controllers;

[ApiController]
[Route("/internal/api/manager/hash/crack")]
public class InternalManagerController(HashCrackService hashCrackService) : ControllerBase
{
    [Consumes("application/xml")]
    [HttpPatch("request")]
    public IActionResult ReceiveResult([FromBody] WorkerTaskResponse response)
    {
        hashCrackService.ProcessWorkerResult(response);
        return Ok();
    }
}