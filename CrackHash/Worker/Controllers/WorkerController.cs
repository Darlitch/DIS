using Contract.Xml;
using Microsoft.AspNetCore.Mvc;

namespace Worker.Controllers;

[ApiController]
[Route("/internal/api/[controller]/hash/crack")]
public class WorkerController : ControllerBase
{
    [HttpPost("task")]
    public IActionResult CrackTask([FromBody] WorkerTaskRequest request)
    {
        return Ok();
    }
}